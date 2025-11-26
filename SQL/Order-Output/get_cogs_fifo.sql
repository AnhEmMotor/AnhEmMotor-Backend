CREATE OR REPLACE FUNCTION get_cogs_fifo (
    p_variant_id uuid,
    p_quantity_to_sell integer,
)

DECLARE
    v_total_cost bigint := 0;
    v_quantity_needed smallint := p_quantity_to_sell;
    v_initial_quantity smallint := p_quantity_to_sell;
    v_batch RECORD;
BEGIN
    -- 1. Kiểm tra đầu vào
    IF v_quantity_needed IS NULL OR v_quantity_needed <= 0 THEN
        RAISE EXCEPTION 'Số lượng bán (quantity_to_sell) phải là một số dương.';
    END IF;

    -- 2. Lặp qua các lô hàng (cũ nhất trước)
    -- FOR UPDATE: Khóa các hàng này để tránh lỗi khi 2 đơn hàng xử lý cùng lúc
    FOR v_batch IN
        SELECT id, input_price, remaining_count
        FROM public.input_info
        WHERE product_code = p_variant_id
          AND remaining_count > 0
        ORDER BY created_at ASC
        FOR UPDATE -- Cực kỳ quan trọng để xử lý đồng thời
    LOOP
        IF v_batch.remaining_count >= v_quantity_needed THEN
            -- Lô này đủ hàng
            v_total_cost := v_total_cost + (v_quantity_needed * v_batch.input_price);
            
            -- Cập nhật số lượng còn lại của lô
            UPDATE public.input_info
            SET remaining_count = remaining_count - v_quantity_needed
            WHERE id = v_batch.id;

            v_quantity_needed := 0; -- Đã đủ hàng
            EXIT; -- Thoát vòng lặp
        ELSE
            -- Lô này không đủ, dùng hết và tiếp tục
            v_total_cost := v_total_cost + (v_batch.remaining_count * v_batch.input_price);
            
            -- Giảm số lượng cần tìm
            v_quantity_needed := v_quantity_needed - v_batch.remaining_count;

            -- Cập nhật lô này về 0
            UPDATE public.input_info
            SET remaining_count = 0
            WHERE id = v_batch.id;
        END IF;
    END LOOP;

    -- 5. Xử lý lỗi (nếu không đủ hàng)
    IF v_quantity_needed > 0 THEN
        RAISE EXCEPTION 'Không đủ hàng tồn kho. Chỉ có thể bán %, thiếu % sản phẩm.', 
                        (v_initial_quantity - v_quantity_needed), v_quantity_needed;
    END IF;

    -- 6. Trả về giá vốn trung bình (đã làm tròn)
    -- Ép kiểu sang numeric để chia số thực, sau đó làm tròn
    RETURN round(v_total_cost::numeric / v_initial_quantity);

END;

$$ LANGUAGE plpgsql;