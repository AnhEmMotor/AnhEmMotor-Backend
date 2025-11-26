CREATE OR REPLACE FUNCTION get_monthly_revenue_profit (
    p_months interger
)

BEGIN
    RETURN QUERY
    WITH month_series AS (
        SELECT generate_series(
            date_trunc('month', NOW() - (p_months - 1) * interval '1 month'),
            date_trunc('month', NOW()),
            '1 month'
        )::date AS month
    )
    SELECT
        ms.month AS report_month,
        COALESCE(SUM(oi.price * oi.count), 0)::bigint AS total_revenue,
        COALESCE(SUM((oi.price - oi.cost_price) * oi.count), 0)::bigint AS total_profit
    FROM month_series AS ms
    LEFT JOIN (
        public.output AS o
        JOIN public.output_status AS ost ON o.status_id = ost.id
        JOIN public.output_info AS oi ON o.id = oi.output_id
    ) ON (
        date_trunc('month', o.created_at) = ms.month
        AND ost.name <> 'Đã huỷ'
    )
    GROUP BY ms.month
    ORDER BY ms.month ASC;
END;

$$ LANGUAGE plpgsql;