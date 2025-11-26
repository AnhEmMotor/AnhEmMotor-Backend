CREATE OR REPLACE FUNCTION get_inputs_by_supplier (
    p_supplier_id uuid,
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
        WHERE
            i.supplier_id = p_supplier_id
        AND
            (p_search IS NULL OR p_search = '' OR
             i.id::text ILIKE '%' || p_search || '%' OR
             i.name_verify ILIKE '%' || p_search || '%')
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
            i.created_at,
            i.input_date,
            i.name_verify,
            ist.name AS status_name,
            i.status_id,
            COALESCE(inv.total_payable, 0) AS total
        FROM
            paginated_inputs pi
        JOIN
            public.input i ON pi.id = i.id
        LEFT JOIN
            public.input_status ist ON i.status_id = ist.id
        LEFT JOIN LATERAL (
            SELECT
                SUM(ii.count * ii.input_price) AS total_payable
            FROM
                public.input_info ii
            WHERE
                ii.input_id = pi.id
        ) inv ON true
        ORDER BY
            i.created_at DESC
    )
    SELECT 
        (SELECT count FROM total),
        COALESCE(jsonb_agg(receipt_details), '[]'::jsonb)
    INTO
        v_total_count,
        v_receipts_json
    FROM
        receipt_details;

    RETURN jsonb_build_object(
        'totalCount', v_total_count,
        'inputs', v_receipts_json
    );
END;
$$ LANGUAGE plpgsql;