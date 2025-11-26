CREATE OR REPLACE FUNCTION get_daily_revenue(p_days integer)
BEGIN
    RETURN QUERY
    WITH date_series AS (
        SELECT generate_series(
            CURRENT_DATE - (p_days - 1) * interval '1 day',
            CURRENT_DATE,
            '1 day'
        )::date AS day
    )
    SELECT
        ds.day AS report_day,
        COALESCE(SUM(oi.price * oi.count), 0)::bigint AS total_revenue
    FROM date_series AS ds
    LEFT JOIN (
        public.output AS o
        JOIN public.output_status AS ost ON o.status_id = ost.id
        JOIN public.output_info AS oi ON o.id = oi.output_id
    ) ON (
        o.created_at::date = ds.day
        AND ost.name <> 'Đã huỷ'
    )
    GROUP BY ds.day
    ORDER BY ds.day ASC;
END;
$$ LANGUAGE plpgsql;