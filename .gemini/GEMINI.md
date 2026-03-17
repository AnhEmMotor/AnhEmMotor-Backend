# Project Instructions

## Yêu cầu chung

- Tuyệt đối không viết comment và log trong code trong bất cứ trường hợp nào.
- Tên biến bắt buộc phải sử dụng tiếng Anh.
- Luôn luôn sử dụng cú pháp C# mới nhất.
- Không bao giờ được tự động cài thư viện cho các dự án, trừ trường hợp tôi yêu cầu. Nếu bạn muốn cài thêm thư viện, phải thông báo qua cho tôi.
- Không bao giờ được đổi nội dung các file appsetting.\*.json trừ trường hợp tôi yêu cầu. Nếu bạn muốn thay đổi, phải thông báo qua cho tôi.
- Tất cả phản hồi đều phải được viết bằng tiếng Việt, kể cả Task, Plan.

## Formatting

- Áp dụng các cài đặt được xác định trong `.editorconfig`.
- Luôn ưu tiên việc thay thế việc gọi tên định danh đầy đủ (fully qualified name) bằng cách import namespace (using) ở đầu tệp tin.
- Chèn một dòng mới trước dấu ngoặc nhọn mở của bất kỳ khối mã nào (ví dụ: sau `if`, `for`, `while`, `foreach`, `using`, `try`, v.v.).
- Đảm bảo rằng câu lệnh return cuối cùng của một phương thức nằm trên một dòng riêng.
- Sử dụng khớp mẫu (pattern matching) và biểu thức switch bất cứ khi nào có thể.
- Sử dụng `nameof` thay vì chuỗi ký tự khi tham chiếu đến tên thành viên.
- Đảm bảo rằng các chú thích tài liệu XML được tạo cho bất kỳ hàm nào và class nào được tạo ra ở trong dự án WebAPI. Khi thích hợp, hãy bao gồm tài liệu `<example>` và `<code>` trong các chú thích.
- Luôn đính kèm CancellationToken và thiết lập ConfigureAwait (true cho WebAPI/Test, false cho các trường hợp khác) để đảm bảo khả năng kiểm soát tiến trình và hiệu suất xử lý luồng.

## Toàn vẹn kiểu dữ liệu và xử lý kiểu dữ liệu null.

- Luôn mặc định biến là non-nullable (không được null). Chỉ dùng ? khi thực sự có logic nghiệp vụ cho phép giá trị đó trống, hoặc trong trường Entity EF Core không phải là khoá chính.
- Chặn đứng tại cửa ngõ (Boundary Validation): Không tin tưởng vào annotation khi nhận dữ liệu từ bên ngoài (API, Database, JSON). Tại các Entry Points (Public Methods, Constructors, Controllers), phải dùng ArgumentNullException.ThrowIfNull để bảo vệ hệ thống ngay lập tức.
- Cấm sử dụng == null hoặc != null. Bắt buộc dùng is null hoặc is not null để tránh các lỗi logic do nạp chồng toán tử (operator overloading).
- Ưu tiên dùng từ khóa required hoặc Constructor để đảm bảo đối tượng luôn đủ dữ liệu. Tuyệt đối không dùng default! để "lừa" trình biên dịch.
- Tuyệt đối không dùng toán tử null-forgiving (!). Việc dùng ! là biểu hiện của sự lười biếng trong xử lý logic. Nếu bạn tin chắc nó không null, hãy chứng minh bằng code check null hoặc gán giá trị mặc định (??).
- Dù kiểu dữ liệu được khai báo là string, nhưng nếu nó đến từ một bản ghi Database cũ hoặc một API bên thứ ba, nó vẫn có thể là null. Phải kiểm tra tính toàn vẹn trước khi xử lý.

## Data Validation

- Luôn sử dụng FluentValidation cho việc xác thực dữ liệu.
- Không sử dụng Data Annotations (như [Required], [StringLength]) cho việc xác thực dữ liệu.
- Khi nhận dữ liệu từ bên ngoài (API, Database, JSON), phải sử dụng FluentValidation để kiểm tra tính toàn vẹn trước khi xử lý.

## Quy trình viết Test (xUnit v3)

- Dự án này viết code sử dụng xUnit SDK v3.
- Khi viết 1 tính năng mới hoặc sửa chữa 1 tính năng đã có, phải hỏi lại để xem có muốn viết test tự động cho tính năng được viết mới/được sửa đó.
- Xác định ID: Trước khi bắt đầu viết bất kỳ test xUnit tự động nào, AI phải hỏi người dùng về ID bắt đầu (ví dụ: TC001, 101, ...). Mọi bài test viết ra sau đó phải tăng dần ID dựa trên mốc này.
- Mọi lần viết thêm bài test phải đi kèm một bảng thống kê trong file Markdown. Bảng phải bao gồm các cột sau:
  - ID: Mã định danh tăng dần.
  - Tiêu đề: Tên ngắn gọn của bài test.
  - Yêu cầu dữ liệu đầu vào: Mô tả tham số hoặc đối tượng truyền vào.
  - Dữ liệu phải có trước (Pre-condition): Trạng thái hệ thống hoặc dữ liệu mẫu cần thiết.
  - Kết quả mong đợi: Trạng thái hoặc giá trị trả về sau khi thực thi.
  - Số lượng Test: Tổng số kịch bản (Case) thực tế sẽ chạy.
  - Loại Test: Phải định rõ là Unit Test, Controller Test hoặc IntegrationTests.
- Quy trình cập nhật bài test (Update):
  - Khi chỉ sửa đổi nhỏ không làm thay đổi bản chất logic, không cần tạo lại bảng.
  - Thay đổi quan trọng: Nếu việc sửa bài test dẫn đến thay đổi quan trọng về logic hoặc dữ liệu, bắt buộc phải viết một file Markdown khác tách biệt. File này phải giữ nguyên cấu trúc bảng như khi tạo mới, trong đó cột ID phải khớp chính xác với ID của bài test đang được sửa đổi và điền đầy đủ nội dung tương ứng, không được bỏ sót bất kỳ cột nào.
- Số lượng bài test và nội dung bài test được sinh ra trong mã nguồn phải khớp chính xác với bảng. Không được viết thừa, thiếu hoặc sai lệch logic so với bảng mô tả.
- Trong thuộc tính [Fact] hoặc [Theory], bắt buộc phải có tham số DisplayName với định dạng chính xác: <ID> - <Tiêu đề bài test>.
- Tuyệt đối không để lại comment trong code. Tên biến phải sử dụng tiếng Anh.
- Tuyệt đối không được sao chép logic xử lý từ code chính vào mã nguồn test để tính toán kết quả mong đợi. Giá trị mong đợi phải là giá trị tĩnh (Static Expected Results) hoặc được xác định độc lập hoàn toàn dựa trên yêu cầu nghiệp vụ. Bài test phải kiểm tra kết quả thực tế dựa trên một "chân lý" bên ngoài, không phải tự suy diễn từ code đang được test.

# Tạo Migration mới cho dự án

- Phải sử dụng file add-migration.ps1 để tạo migraion mới. Cú pháp

```
./add-migration.ps1 -MigrationName "TenMigrationCuaBan"
```

# Cấu trúc Response cho Tanstack Query (Frontend Support)

- Để hỗ trợ quy trình CRUD chuẩn ở Frontend, Backend phải tuân thủ:
- Create/Update (Mutation Support): \* Sau khi thực hiện thành công, Action phải trả về toàn bộ Object (DTO) vừa được tạo hoặc cập nhật, không chỉ trả về ID hay Boolean.
- Dữ liệu trả về phải chứa đầy đủ các trường thông tin cần thiết để Frontend có thể gọi queryClient.setQueryData cập nhật cache tức thì.
- Get Detail: Đảm bảo cấu trúc dữ liệu trả về của phương thức lấy chi tiết đồng nhất với dữ liệu trả về của phương thức Create/Update.
- Delete: Trả về trạng thái thành công rõ ràng để Frontend thực hiện removeQueries và invalidateQueries.

## Build và Test

- Mỗi khi có sự thay đổi trong code chạy dự án, bắt buộc phải build và test dự án.
- Trước khi bắt đầu Build và Test, phải thực hiện đóng sạch (Force Stop) tất cả các tiến trình dotnet đang vận hành trên hệ thống. Việc này nhằm giải phóng hoàn toàn các tệp tin đang bị khóa (File Locking) và làm trống bộ nhớ đệm của trình biên dịch.
- Kiểm tra biên dịch: Chạy câu lệnh dotnet build để kiểm tra xem có cảnh báo và lỗi cú pháp hay không.
- Ngay sau đó, Chạy câu lệnh dotnet test với mức độ chi tiết normal. Toàn bộ dữ liệu đầu ra phải hiển thị trên cửa sổ lệnh, chỉ khi nào cửa sổ lệnh hoàn tất thì mới được tiếp tục sang bước tiếp theo.
- Kiểm tra kết quả: Sau khi lệnh test dự án thành công, buộc phải đọc cửa sổ lệnh sau khi xong để xem coi kết quả. Nếu như kết quả thất bại khi build hoặc khi test, buộc phải sửa lại lỗi dựa trên lỗi được chỉ ra dựa trên cửa sổ lệnh đó cho đến khi file kết quả không có bất kì lỗi và cảnh báo nào (Đặc biệt là tuyệt đối không có bất kì cảnh báo nào). Trong quá trình sửa tuyệt đối không được làm mất logic test, đặc biệt là logic kiểm tra kết quả bài test đó.

## Tuyệt đối không tự ý Commit Dự án

Tuyệt đối không tự ý commit và push code lên trên git cho dự án.
