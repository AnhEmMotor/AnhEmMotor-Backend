CREATE OR REPLACE FUNCTION get_all_orders (
    p_page integer,
    p_items_per_page integer,
    p_status_ids text[],
    p_search text
)
DECLARE
    v_offset integer;
    v_limit integer;
    v_total_count integer;
    v_orders_json jsonb;
BEGIN
    -- Tính toán offset và limit
    v_limit := p_items_per_page;
    v_offset := (p_page - 1) * p_items_per_page;

    -- CTE để lọc và phân trang đơn hàng
    WITH FilteredOutputs AS (
        SELECT
            o.id,
            o.created_at,
            o.customer_name,
            o.emp_code,
            o.status_id
        FROM
            public.output o
        WHERE
            -- Lọc theo status
            (p_status_ids IS NULL OR o.status_id = ANY(p_status_ids))
            AND
            -- Lọc theo search (tên khách hàng)
            (p_search IS NULL OR p_search = '' OR o.customer_name ILIKE '%' || p_search || '%')
    ),
    -- Đếm tổng số lượng
    TotalCount AS (
        SELECT COUNT(*) AS count FROM FilteredOutputs
    ),
    -- Lấy chi tiết sản phẩm và tính toán
    PaginatedOutputs AS (
        SELECT
            fo.id,
            fo.created_at,
            fo.customer_name,
            fo.emp_code,
            fo.status_id,
            -- Tính tổng tiền
            COALESCE(SUM(oi.price * oi.count), 0) AS total,
            -- Tạo tóm tắt sản phẩm
            COALESCE(STRING_AGG(pv.name, ', '), 'Không có sản phẩm') AS product_summary,
            -- Lấy chi tiết sản phẩm dưới dạng JSON
            COALESCE(
                jsonb_agg(jsonb_build_object(
                    'id', oi.id,
                    'product_id', oi.product_id,
                    'name', pv.name,
                    'count', oi.count,
                    'price', oi.price,
                    'cost_price', oi.cost_price
                )) FILTER (WHERE oi.id IS NOT NULL),
                '[]'::jsonb
            ) AS products
        FROM
            FilteredOutputs fo
        LEFT JOIN
            public.output_info oi ON fo.id = oi.output_id
        LEFT JOIN
            -- Lấy tên sản phẩm
            (
                SELECT
                    pv_sub.id,
                    p_sub.name || ' (' || COALESCE(vot_sub.options_text, '') || ')' AS name
                FROM
                    public.product_variants pv_sub
                JOIN
                    public.product p_sub ON pv_sub.product_id = p_sub.id
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
                ) vot_sub ON pv_sub.id = vot_sub.variant_id
            ) pv ON oi.product_id = pv.id
        GROUP BY
            fo.id, fo.created_at, fo.customer_name, fo.emp_code, fo.status_id
        ORDER BY
            fo.created_at DESC
        LIMIT v_limit
        OFFSET v_offset
    )
    -- Xây dựng JSON trả về
    SELECT
        jsonb_build_object(
            'totalCount', (SELECT count FROM TotalCount),
            'orders', COALESCE(jsonb_agg(po.*), '[]'::jsonb)
        )
    INTO
        v_orders_json
    FROM
        PaginatedOutputs po;

    RETURN v_orders_json;
END;
$$ LANGUAGE plpgsql;