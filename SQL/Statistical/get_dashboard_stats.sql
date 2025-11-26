CREATE OR REPLACE FUNCTION get_dashboard_stats

DECLARE
    v_last_month_revenue NUMERIC;
    v_last_month_profit NUMERIC;
    v_pending_orders_count BIGINT;
    v_new_customers_count BIGINT := 0; -- placeholder
BEGIN
    v_last_month_revenue := (
        SELECT COALESCE(SUM(oi.price * oi.count), 0)
        FROM public.output_info AS oi
        JOIN public.output AS o ON oi.output_id = o.id
        JOIN public.output_status AS ost ON o.status_id = ost.id
        WHERE o.created_at >= date_trunc('month', now() - interval '1 month')
          AND o.created_at < date_trunc('month', now())
          AND ost.name <> 'Đã huỷ'
    );

    v_last_month_profit := (
        SELECT COALESCE(SUM((oi.price - oi.cost_price) * oi.count), 0)
        FROM public.output_info AS oi
        JOIN public.output AS o ON oi.output_id = o.id
        JOIN public.output_status AS ost ON o.status_id = ost.id
        WHERE o.created_at >= date_trunc('month', now() - interval '1 month')
          AND o.created_at < date_trunc('month', now())
          AND ost.name <> 'Đã huỷ'
    );

    v_pending_orders_count := (
        SELECT COUNT(o.id)
        FROM public.output AS o
        JOIN public.output_status AS ost ON o.status_id = ost.id
        WHERE ost.name = 'chưa xác nhận'
    );

    -- Trả về kết quả đã ép kiểu cẩn thận
    RETURN QUERY SELECT
        v_last_month_revenue::BIGINT,
        v_last_month_profit::BIGINT,
        v_pending_orders_count,
        v_new_customers_count;
END;

$$ LANGUAGE plpgsql;