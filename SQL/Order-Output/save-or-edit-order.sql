CREATE OR REPLACE FUNCTION save_order (
    p_order_id uuid,
    p_customer_name text,
    p_notes text,
    p_status_id text,
    p_products jsonb
)

DECLARE
    v_output_id uuid;
    v_product_item jsonb;
    v_new_order_data jsonb;
BEGIN
    -- Tắt RLS
    SET LOCAL auth.role = 'service_role';

    -- Logic: Tạo mới hay Cập nhật?
    IF p_order_id IS NULL THEN
        -- TẠO MỚI ĐƠN HÀNG (INSERT)
        INSERT INTO public.output (customer_name, notes, status_id)
        VALUES (p_customer_name, p_notes, p_status_id)
        RETURNING id INTO v_output_id;
    ELSE
        -- CẬP NHẬT ĐƠN HÀNG (UPDATE)
        v_output_id := p_order_id;

        UPDATE public.output
        SET
            customer_name = p_customer_name,
            notes = p_notes,
            status_id = p_status_id
        WHERE
            id = v_output_id;

        -- Xóa các chi tiết cũ
        DELETE FROM public.output_info
        WHERE output_id = v_output_id;
    END IF;

    -- Thêm (hoặc thêm lại) các sản phẩm trong chi tiết
    IF p_products IS NOT NULL AND jsonb_array_length(p_products) > 0 THEN
        FOR v_product_item IN SELECT * FROM jsonb_array_elements(p_products)
        LOOP
            INSERT INTO public.output_info (
                output_id,
                product_id,
                count,
                price,
                cost_price
            )
            VALUES (
                v_output_id,
                (v_product_item->>'product_id')::uuid,
                (v_product_item->>'count')::smallint,
                (v_product_item->>'price')::bigint,
                (v_product_item->>'cost_price')::bigint -- Giả sử cost_price lấy từ form, nếu không thì lấy từ bảng khác
            );
        END LOOP;
    END IF;
    
    -- Bật lại RLS
    RESET auth.role;

    -- Trả về dữ liệu đơn hàng mới nhất
    -- (Tái sử dụng logic của get_all_orders cho 1 record)
    SELECT
        jsonb_build_object(
            'id', o.id,
            'created_at', o.created_at,
            'customer_name', o.customer_name,
            'emp_code', o.emp_code,
            'status_id', o.status_id,
            'notes', o.notes,
            'total', COALESCE(SUM(oi.price * oi.count), 0),
            'product_summary', COALESCE(STRING_AGG(pv.name, ', '), 'Không có sản phẩm'),
            'products', COALESCE(
                jsonb_agg(jsonb_build_object(
                    'id', oi.id,
                    'product_id', oi.product_id,
                    'name', pv.name,
                    'count', oi.count,
                    'price', oi.price,
                    'cost_price', oi.cost_price
                )) FILTER (WHERE oi.id IS NOT NULL),
                '[]'::jsonb
            )
        )
    INTO
        v_new_order_data
    FROM
        public.output o
    LEFT JOIN
        public.output_info oi ON o.id = oi.output_id
    LEFT JOIN (
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
    WHERE
        o.id = v_output_id
    GROUP BY
        o.id, o.created_at, o.customer_name, o.emp_code, o.status_id, o.notes;

    RETURN v_new_order_data;
END;

$$ LANGUAGE plpgsql;