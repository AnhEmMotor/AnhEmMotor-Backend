CREATE OR REPLACE FUNCTION get_product_report_last_month

DECLARE
    v_start_last_month date := date_trunc('month', now() - interval '1 month');
    v_end_last_month date := date_trunc('month', now());
BEGIN
    RETURN QUERY
    WITH
    -- 1. Tổng Nhập "Đã xác nhận" (tính Tồn kho)
    confirmed_inputs AS (
        SELECT
            ii.product_code AS variant_id,
            COALESCE(SUM(ii.count), 0) AS total_in
        FROM public.input_info AS ii
        JOIN public.input AS i ON ii.input_id = i.id
        JOIN public.input_status AS ist ON i.status_id = ist.id
        WHERE ist.name = 'Đã xác nhận'
        GROUP BY ii.product_code
    ),
    -- 2. Tổng Xuất "Không huỷ" - Toàn thời gian (tính Tồn kho)
    sold_outputs_all_time AS (
        SELECT
            oi.product_id AS variant_id,
            COALESCE(SUM(oi.count), 0) AS total_out
        FROM public.output_info AS oi
        JOIN public.output AS o ON oi.output_id = o.id
        JOIN public.output_status AS ost ON o.status_id = ost.id
        WHERE ost.name <> 'Đã huỷ'
        GROUP BY oi.product_id
    ),
    -- 3. Tổng Xuất "Không huỷ" - Tháng trước (tính Đã bán)
    sold_outputs_last_month AS (
        SELECT
            oi.product_id AS variant_id,
            COALESCE(SUM(oi.count), 0) AS total_sold
        FROM public.output_info AS oi
        JOIN public.output AS o ON oi.output_id = o.id
        JOIN public.output_status AS ost ON o.status_id = ost.id
        WHERE ost.name <> 'Đã huỷ'
          AND o.created_at >= v_start_last_month
          AND o.created_at < v_end_last_month
        GROUP BY oi.product_id
    )
    -- 4. Kết hợp dữ liệu
    SELECT
        p.name AS product_name,
        pv.id AS variant_id,
        -- Tồn kho = (Tổng Nhập) - (Tổng Xuất toàn thời gian)
        COALESCE(ci.total_in, 0) - COALESCE(so_all.total_out, 0) AS stock_quantity,
        -- Đã bán tháng trước
        COALESCE(so_lm.total_sold, 0) AS sold_last_month
    FROM public.product_variants AS pv
    JOIN public.product AS p ON pv.product_id = p.id
    LEFT JOIN confirmed_inputs AS ci ON pv.id = ci.variant_id
    LEFT JOIN sold_outputs_all_time AS so_all ON pv.id = so_all.variant_id
    LEFT JOIN sold_outputs_last_month AS so_lm ON pv.id = so_lm.variant_id
    ORDER BY p.name;
END;

$$ LANGUAGE plpgsql;