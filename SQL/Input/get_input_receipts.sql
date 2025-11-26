CREATE OR REPLACE FUNCTION get_input_receipts (
    p_page integer,
    p_items_per_page integer,
    p_status_ids text[],
    p_search text
)
DECLARE
    v_offset integer;
    v_total_count integer;
    v_receipts_json jsonb;
BEGIN
    v_offset := (p_page - 1) * p_items_per_page;

    WITH filtered_inputs AS (
        SELECT 
            i.id, i.created_at
        FROM 
            public.input i
        LEFT JOIN 
            public.supplier s ON i.supplier_id = s.id
        WHERE
            (p_search IS NULL OR p_search = '' OR
             i.id::text ILIKE '%' || p_search || '%' OR
             s.name ILIKE '%' || p_search || '%' OR
             s.id::text ILIKE '%' || p_search || '%')
        AND
            (p_status_ids IS NULL OR cardinality(p_status_ids) = 0 OR
             i.status_id = ANY(p_status_ids))
    ),
    total AS (
        SELECT COUNT(*) FROM filtered_inputs
    ),
    paginated_inputs AS (
        SELECT id FROM filtered_inputs
        ORDER BY created_at DESC
        LIMIT p_items_per_page
        OFFSET v_offset
    ),
    receipt_details AS (
        SELECT
            i.id,
            i.created_at AS time,
            s.id AS "supplierCode",
            s.name AS "supplierName",
            COALESCE(inv.total_payable, 0) AS payable,
            i.status_id AS status,
            i.notes AS notes,
            i.input_date AS "importDate",
            0 AS paid,
            COALESCE(inv.products, '[]'::jsonb) AS products
        FROM
            paginated_inputs pi
        JOIN
            public.input i ON pi.id = i.id
        LEFT JOIN
            public.supplier s ON i.supplier_id = s.id
        LEFT JOIN LATERAL (
            SELECT
                jsonb_agg(
                    jsonb_build_object(
                        'code', pv.id,
                        'name', p.name || ' (' || COALESCE(vot.options_text, '') || ')',
                        'quantity', ii.count,
                        'unitPrice', ii.input_price,
                        'discount', 0, 
                        'importPrice', ii.input_price,
                        'total', ii.count * ii.input_price
                    )
                ) AS products,
                SUM(ii.count * ii.input_price) AS total_payable
            FROM
                public.input_info ii
            JOIN
                public.product_variants pv ON ii.product_code = pv.id
            JOIN
                public.product p ON pv.product_id = p.id
            LEFT JOIN (
                SELECT
                    vov.variant_id,
                    string_agg(o.name || ': ' || ov.name, ', ' ORDER BY o.name) AS options_text
                FROM
                    public.variant_option_values vov
                JOIN
                    public.option_values ov ON vov.option_value_id = ov.id
                JOIN
                    public.options o ON ov.option_id = o.id
                GROUP BY
                    vov.variant_id
            ) vot ON pv.id = vot.variant_id
            WHERE
                ii.input_id = pi.id
        ) inv ON true
        ORDER BY
            i.created_at DESC
    )
    SELECT 
        (SELECT count FROM total),
        COALESCE(jsonb_agg(rd ORDER BY rd.time DESC), '[]'::jsonb)
    INTO
        v_total_count,
        v_receipts_json
    FROM
        receipt_details rd;

    RETURN jsonb_build_object(
        'totalCount', v_total_count,
        'data', v_receipts_json
    );
END;
$$ LANGUAGE plpgsql;