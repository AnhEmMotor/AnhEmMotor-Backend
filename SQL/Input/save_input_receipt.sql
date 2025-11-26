CREATE OR REPLACE FUNCTION save_input_receipt (
    p_input_id uuid,
    p_supplier_id uuid,
    p_status_id text,
    p_name_verify text,
    p_import_date timestamp,
    p_notes text,
    p_paid bigint,
    p_products jsonb
)
DECLARE
    v_input_id uuid;
    v_product_row RECORD;
BEGIN
    IF p_input_id IS NULL THEN
        INSERT INTO public.input (supplier_id, status_id, notes, input_date)
        VALUES (p_supplier_id, p_status_id, p_notes, p_import_date)
        RETURNING id INTO v_input_id;
    ELSE
        UPDATE public.input
        SET
            supplier_id = p_supplier_id,
            status_id = p_status_id,
            input_date = p_import_date,
            notes = p_notes
        WHERE
            id = p_input_id
        RETURNING id INTO v_input_id;
    END IF;

    DELETE FROM public.input_info
    WHERE input_id = v_input_id;

    FOR v_product_row IN 
        SELECT 
            (item->>'code')::uuid AS product_code,
            (item->>'quantity')::smallint AS count,
            (item->>'unitPrice')::bigint AS input_price
        FROM 
            jsonb_array_elements(p_products) AS item
    LOOP
        IF v_product_row.count > 0 AND v_product_row.input_price >= 0 THEN
             INSERT INTO public.input_info (input_id, product_code, count, input_price, remaining_count)
             VALUES (v_input_id, v_product_row.product_code, v_product_row.count, v_product_row.input_price, v_product_row.count);
        END IF;
    END LOOP;

    RETURN v_input_id;
END;
$$ LANGUAGE plpgsql;