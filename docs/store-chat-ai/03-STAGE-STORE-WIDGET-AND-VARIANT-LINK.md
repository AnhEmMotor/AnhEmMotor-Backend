# Stage 03 — Giao diện chat & liên kết biến thể trên Store

> Ưu tiên: 🟠 Trung bình-cao · Ước lượng: 2 ngày · Phụ thuộc: **Stage 01, 02**
> Mục tiêu: `FloatingContact.vue` chat thật với AI, hiển thị card sản phẩm/biến thể, bấm card ra đúng
> trang đúng màu.

---

## 3.1. Nối `FloatingContact.vue` với hạ tầng thật

File: `AnhEmMotor-Store/app/components/common/FloatingContact.vue`.

Hiện trạng: tin nhắn hard-code trong template (dòng 62–114), input không có `v-model`, nút gửi không
có `@click`. Việc cần làm:

- Thêm composable mới `app/composables/useStoreChat.js` (theo pattern các composable hiện có như
  `useCart.js`) — quản lý: sinh/đọc `VisitorKey` từ `localStorage`, kết nối `StoreChatHub` (tái dùng
  cách project đã kết nối SignalR ở Management nếu Store chưa từng dùng SignalR — kiểm tra
  `package.json` của Store xem đã có `@microsoft/signalr` chưa, thêm nếu chưa).
- Thay khối tin nhắn hard-code bằng `v-for` trên danh sách tin nhắn thật từ composable.
- Input gắn `v-model`, nút gửi gọi `sendMessage()` của composable (gửi qua Hub, không qua REST — theo
  quyết định ở Stage 01).
- Giữ nguyên toàn bộ animation/vị trí/style đang có — Stage này chỉ thay **dữ liệu và hành vi**, không
  vẽ lại giao diện.

---

## 3.2. Render card sản phẩm & card biến thể

Thêm 2 component mới trong `app/components/common/` (hoặc `chat/` nếu tách thư mục riêng cho gọn):

- `StoreChatProductCard.vue` — nhận `{ productId, name, imageUrl, priceFrom, priceTo }`, bấm vào thì
  emit sự kiện yêu cầu xem biến thể (gọi tiếp AI hỏi biến thể, hoặc nếu payload đã kèm sẵn variant đầu
  tiên thì điều hướng thẳng).
- `StoreChatVariantCard.vue` — nhận `{ productId, variantId, colorName, sku, price }`, bấm vào điều
  hướng thẳng tới trang sản phẩm kèm query param biến thể (mục 3.3).

Trong template chat, tin nhắn AI có `kind: "product-cards"`/`"variant-cards"` (payload định nghĩa ở
Stage 02 mục 2.3) render bằng 2 component trên thay vì bong bóng text.

---

## 3.3. Deep-link biến thể trên trang sản phẩm

File: `AnhEmMotor-Store/app/pages/product/ProductDetail.vue`.

Hiện trạng (dòng 8–9): `const slug = computed(() => route.params.slug)`, chọn biến thể qua
`selectedVariantGroup` (dòng 201+) hoàn toàn bằng tương tác tay, không đọc từ URL.

Thêm:

```js
const route = useRoute();
const requestedVariantId = computed(() => route.query.variant ? Number(route.query.variant) : null);

onMounted(() => {
  if (requestedVariantId.value) {
    const group = findVariantGroupContaining(requestedVariantId.value); // tìm trong variantGroups.value đã có
    if (group) selectedVariantGroup.value = group;
  }
});
```

Card biến thể ở chat điều hướng bằng `navigateTo(`/san-pham/${slug}?variant=${variantId}`)` (giữ đúng
pattern route hiện có, chỉ thêm query param `variant`).

---

## Definition of Done — Stage 03

- [ ] `FloatingContact.vue` chat thật, không còn tin nhắn hard-code, không mất animation/vị trí hiện có.
- [ ] Card sản phẩm và card biến thể render đúng, bấm được.
- [ ] Bấm card biến thể → mở đúng `ProductDetail.vue`, đúng sản phẩm, đúng màu đã chọn sẵn (kiểm chứng
      bằng `Claude_Browser`, không chỉ đọc code).
- [ ] Đóng mở lại chat trong cùng phiên duyệt web → vẫn thấy lịch sử chat cũ (dùng lại `VisitorKey`
      từ Stage 01).
