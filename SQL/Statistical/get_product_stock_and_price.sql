CREATE OR REPLACE FUNCTION get_product_stock_and_price (
    p_variant_id uuid
)

DECLARE
    v_total_input BIGINT;
    v_total_output BIGINT;
    v_price BIGINT;
BEGIN
    -- 1. Lấy đơn giá (Theo yêu cầu mới: Join bảng price)
    -- Lấy giá mới nhất nếu có nhiều giá
    SELECT p.product_price
    INTO v_price
    FROM public.price AS p
    WHERE p.product_id = p_variant_id
    ORDER BY p.created_at DESC
    LIMIT 1;

    -- 2. Tính Tổng Nhập
    -- Chỉ tính các phiếu nhập đã ở trạng thái 'Đã xác nhận' hoặc 'Đã hoàn thành'
    -- (Dựa theo ghi chú "chứ méo phải chưa có xác nhận")
    SELECT COALESCE(SUM(ii.count), 0)
    INTO v_total_input
    FROM public.input_info AS ii
    JOIN public.input AS i ON ii.input_id = i.id
    JOIN public.input_status AS ist ON i.status_id = ist.id
    WHERE ii.product_code = p_variant_id
      AND ist.name IN ('Đã xác nhận', 'Đã hoàn thành', 'Đã nhập');

    -- 3. Tính Tổng Xuất (Theo yêu cầu mới)
    -- Chỉ tính khi trạng thái là 'Đã giao', 'đã xác nhận', 'đã hoàn thành'
    SELECT COALESCE(SUM(oi.count), 0)
    INTO v_total_output
    FROM public.output_info AS oi
    JOIN public.output AS o ON oi.output_id = o.id
    JOIN public.output_status AS ost ON o.status_id = ost.id
    WHERE oi.product_id = p_variant_id
      AND ost.name IN ('Đã giao', 'đã xác nhận', 'đã hoàn thành');

    -- 4. Trả về kết quả
    RETURN QUERY SELECT 
        COALESCE(v_price, 0) AS unit_price,
        (v_total_input - v_total_output) AS stock_quantity;

END;

$$ LANGUAGE plpgsql;