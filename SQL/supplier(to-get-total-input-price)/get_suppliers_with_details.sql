CREATE OR REPLACE FUNCTION get_suppliers_with_details (
    p_page integer,
    p_items_per_page integer,
    p_status_ids text[],
    p_search text
)

DECLARE
    v_start INT;
    v_search_term TEXT;
BEGIN
    -- Tính toán offset cho phân trang
    v_start := (p_page - 1) * p_items_per_page;
    -- Chuẩn bị 'term' cho tìm kiếm ILIKE
    v_search_term := '%' || p_search || '%';

    RETURN QUERY
    WITH 
    -- 1. Tính tổng tiền cho mỗi đơn nhập hàng (input)
    input_sums AS (
        SELECT
            ii.input_id,
            SUM(COALESCE(ii.count, 0) * COALESCE(ii.input_price, 0)) AS input_total
        FROM
            input_info AS ii
        GROUP BY
            ii.input_id
    ),
    supplier_totals AS (
        SELECT
            i.supplier_id,
            SUM(COALESCE(isum.input_total, 0)) AS total_purchase
        FROM
            input AS i
        JOIN
            input_sums AS isum ON i.id = isum.input_id
        WHERE
            i.status_id = 'finished'
        GROUP BY
            i.supplier_id
    ),
    filtered_suppliers AS (
        SELECT
            s.id,
            s.name,
            s.phone,
            s.email,
            s.address,
            s.notes,
            s.deleted_at,
            s.created_at,
            s.status_id AS status,
            COALESCE(st.total_purchase, 0) AS total_purchase,
            COUNT(*) OVER() AS total_count
        FROM
            supplier AS s
        LEFT JOIN
            supplier_totals AS st ON s.id = st.supplier_id
        WHERE
            s.deleted_at IS NULL
            AND (
                p_search IS NULL OR p_search = '' OR
                (s.name ILIKE v_search_term OR s.phone ILIKE v_search_term OR s.email ILIKE v_search_term)
            )
            AND (
                p_status_ids IS NULL OR
                s.status_id = ANY(p_status_ids) -- So sánh tên status (text)
            )
    )
    SELECT 
        fs.id,
        fs.name,
        fs.phone,
        fs.email,
        fs.address,
        fs.notes,
        fs.deleted_at,
        fs.created_at,
        fs.status,
        fs.total_purchase::numeric, 
        fs.total_count
    FROM filtered_suppliers AS fs
    ORDER BY fs.name ASC
    OFFSET v_start
    LIMIT p_items_per_page;

END;

$$ LANGUAGE plpgsql;