CREATE OR REPLACE FUNCTION get_order_status_counts
BEGIN
    RETURN QUERY
    SELECT
        ost.name AS status_name,
        COUNT(o.id) AS order_count
    FROM public.output_status AS ost
    -- LEFT JOIN để đảm bảo hiển thị cả trạng thái có 0 đơn hàng
    LEFT JOIN public.output AS o ON ost.id = o.status_id
    GROUP BY ost.name
    ORDER BY ost.name;
END;

$$ LANGUAGE plpgsql;