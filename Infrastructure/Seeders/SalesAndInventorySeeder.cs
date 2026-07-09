using Domain.Constants.Order;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Infrastructure.Seeders
{
    public static class SalesAndInventorySeeder
    {
        public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
        {
            var variants = await context.ProductVariants
                .Include(v => v.ProductVariantColors)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            if (variants.Count == 0)
                return;
            if (await context.OutputOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }
            var now = DateTimeOffset.UtcNow;
            var today = now.Date;
            var supplier = await context.Suppliers.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var supplierId = supplier?.Id;
            var random = new Random(42);
            var statuses = new[]
            {
                OrderStatus.Completed,
                OrderStatus.Completed,
                OrderStatus.Completed,
                OrderStatus.Delivering,
                OrderStatus.WaitingPickup,
                OrderStatus.Pending,
                OrderStatus.Cancelled,
                OrderStatus.WaitingDeposit
            };
            var salesUsers = await context.Users
                .Where(
                    u => u.Email == "nguyen.van.a@anhemmotor.com" ||
                        u.Email == "tran.thi.b@anhemmotor.com" ||
                        u.Email == "pham.thi.d@anhemmotor.com")
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
            var seededOutputs = new List<Output>();
            for (int i = 11; i >= 0; i--)
            {
                var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-i);
                int ordersInMonth = random.Next(10, 25);
                for (int o = 0; o < ordersInMonth; o++)
                {
                    var orderDate = monthStart.AddDays(random.Next(1, 28)).AddHours(random.Next(8, 20));
                    var status = OrderStatus.Completed;
                    if (i == 0)
                    {
                        status = statuses[random.Next(statuses.Length)];
                    }
                    var output = new Output
                    {
                        CustomerName = $"Khách hàng {random.Next(100, 999)}",
                        CustomerPhone = $"090{random.Next(1000000, 9999999)}",
                        CustomerAddress = $"{random.Next(1, 500)} Đường Láng, Hà Nội",
                        CreatedAt = orderDate,
                        UpdatedAt = orderDate,
                        StatusId = status,
                        PaymentStatus = status == OrderStatus.Completed ? "Paid" : "Unpaid",
                        PaymentMethod = status == OrderStatus.Completed ? "Banking" : "COD",
                        DepositRatio = 10,
                        LastStatusChangedAt = orderDate
                    };
                    if (salesUsers.Count > 0 && status == OrderStatus.Completed)
                    {
                        output.FinishedBy = salesUsers[random.Next(salesUsers.Count)].Id;
                    }
                    context.OutputOrders.Add(output);
                    seededOutputs.Add(output);
                }
            }
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            foreach (var output in seededOutputs)
            {
                var orderDate = output.CreatedAt ?? now;
                var variant = variants[random.Next(variants.Count)];
                var color = variant.ProductVariantColors.FirstOrDefault();
                var qty = random.Next(1, 3);
                var price = variant.Price ?? 30000000;
                var costPrice = price * 0.8m;
                var outputInfo = new OutputInfo
                {
                    OutputId = output.Id,
                    ProductVariantId = variant.Id,
                    ProductVariantColorId = color?.Id,
                    Count = qty,
                    Price = price,
                    CostPrice = costPrice,
                    CreatedAt = orderDate,
                    UpdatedAt = orderDate
                };
                context.OutputInfos.Add(outputInfo);
            }
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            if (!await context.RepairOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var technicians = await context.EmployeeProfiles.ToListAsync(cancellationToken).ConfigureAwait(false);
                for (int i = 0; i < 4; i++)
                {
                    var ticketDate = now.AddHours(-random.Next(1, 12));
                    var tech = technicians.Count > 0 ? technicians[random.Next(technicians.Count)] : null;
                    var ro = new RepairOrder
                    {
                        CustomerName = $"Nguyễn Anh Tuấn {i + 1}",
                        CustomerPhone = $"091{random.Next(1000000, 9999999)}",
                        Description = "Kiểm tra định kỳ, thay dầu nhớt động cơ và vệ sinh bộ côn xe ga",
                        Status = "InProgress",
                        StartTime = ticketDate,
                        ExpectedCompletionTime = now.AddHours(random.Next(1, 6)),
                        LaborCost = 150000,
                        PartsCost = 450000,
                        TotalAmount = 600000,
                        PaymentStatus = "Unpaid",
                        TechnicianId = tech?.Id,
                        CreatedAt = ticketDate,
                        UpdatedAt = ticketDate
                    };
                    context.RepairOrders.Add(ro);
                }
                for (int i = 0; i < 20; i++)
                {
                    var ticketDate = now.AddDays(-random.Next(1, 30)).AddHours(random.Next(8, 18));
                    var durationHours = random.Next(1, 4);
                    var completionDate = ticketDate.AddHours(durationHours);
                    var tech = technicians.Count > 0 ? technicians[random.Next(technicians.Count)] : null;
                    var ro = new RepairOrder
                    {
                        CustomerName = $"Trần Thanh Sơn {i + 1}",
                        CustomerPhone = $"093{random.Next(1000000, 9999999)}",
                        Description = "Bảo dưỡng toàn bộ xe máy, thay lọc gió, bugi và cặp má phanh trước sau",
                        Status = "Completed",
                        StartTime = ticketDate,
                        CompletedDate = completionDate,
                        ExpectedCompletionTime = ticketDate.AddHours(3),
                        LaborCost = 300000,
                        PartsCost = 650000,
                        TotalAmount = 950000,
                        PaymentStatus = "Paid",
                        PaymentMethod = "Banking",
                        TechnicianId = tech?.Id,
                        CreatedAt = ticketDate,
                        UpdatedAt = completionDate
                    };
                    context.RepairOrders.Add(ro);
                }
                for (int i = 0; i < 3; i++)
                {
                    var ticketDate = now.AddHours(-15);
                    var tech = technicians.Count > 0 ? technicians[random.Next(technicians.Count)] : null;
                    var ro = new RepairOrder
                    {
                        CustomerName = $"Lê Minh Hoàng {i + 1}",
                        CustomerPhone = $"098{random.Next(1000000, 9999999)}",
                        Description = "Khắc phục lỗi xước xát nhựa sườn, căn chỉnh phuộc nhún trước",
                        Status = "Pending",
                        StartTime = ticketDate,
                        ExpectedCompletionTime = now.AddHours(-2),
                        LaborCost = 400000,
                        PartsCost = 1500000,
                        TotalAmount = 1900000,
                        PaymentStatus = "Unpaid",
                        TechnicianId = tech?.Id,
                        CreatedAt = ticketDate,
                        UpdatedAt = ticketDate
                    };
                    context.RepairOrders.Add(ro);
                }
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            if (!await context.Contacts.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var adminUser = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == "admin@anhem.com", cancellationToken)
                    .ConfigureAwait(false);
                var adminId = adminUser?.Id;
                var contacts = new List<Contact>
                {
                    new Contact
                    {
                        FullName = "Nguyễn Hoàng Nam",
                        Email = "nam.nguyen12@gmail.com",
                        PhoneNumber = "0948123456",
                        Subject = "Hỏi về chế độ bảo hành xe Honda SH",
                        Message =
                            "Tôi mới mua xe SH ở showroom tuần trước, cho hỏi chế độ bảo hành xe định kỳ như thế nào?",
                        Status = "Replied",
                        Rating = 5,
                        InternalNote = "Khách hàng thân thiết, cần hỗ trợ chu đáo.",
                        CreatedAt = now.AddDays(-10),
                        UpdatedAt = now.AddDays(-9)
                    },
                    new Contact
                    {
                        FullName = "Phạm Minh Trí",
                        Email = "tri.pham@yahoo.com",
                        PhoneNumber = "0918765432",
                        Subject = "Khiếu nại về thái độ phục vụ của nhân viên kỹ thuật",
                        Message =
                            "Nhân viên kỹ thuật lúc bảo dưỡng xe Winner X của tôi thái độ rất không hợp tác, không chịu kiểm tra kỹ xích tải.",
                        Status = "Closed",
                        Rating = 2,
                        InternalNote = "Đã xin lỗi khách hàng và nhắc nhở thợ kỹ thuật.",
                        CreatedAt = now.AddDays(-5),
                        UpdatedAt = now.AddDays(-4)
                    },
                    new Contact
                    {
                        FullName = "Lê Thị Hồng",
                        Email = "hong.le@gmail.com",
                        PhoneNumber = "0987111222",
                        Subject = "Đăng ký mua trả góp xe Vision",
                        Message =
                            "Tôi muốn mua trả góp xe Vision bản đặc biệt, cần làm những thủ tục gì và trả trước bao nhiêu?",
                        Status = "Pending",
                        Rating = null,
                        InternalNote = "Đã giao cho bộ phận Sales gọi điện tư vấn.",
                        CreatedAt = now.AddHours(-12),
                        UpdatedAt = now.AddHours(-12)
                    },
                    new Contact
                    {
                        FullName = "Trần Thanh Hải",
                        Email = "hai.tran@gmail.com",
                        PhoneNumber = "0976555444",
                        Subject = "Yêu cầu thay thế phụ tùng chính hãng",
                        Message = "Tôi muốn thay má phanh trước xe SH 150i, bên cửa hàng có sẵn hàng zin không?",
                        Status = "Replied",
                        Rating = 4,
                        InternalNote = "Đã báo giá má phanh zin Honda.",
                        CreatedAt = now.AddDays(-3),
                        UpdatedAt = now.AddDays(-2)
                    },
                    new Contact
                    {
                        FullName = "Nguyễn Thị Mai",
                        Email = "mai.nguyen@outlook.com",
                        PhoneNumber = "0934123987",
                        Subject = "Góp ý về chất lượng phòng chờ showroom",
                        Message = "Phòng chờ hơi nóng và không có nước uống cho khách hàng ngồi chờ sửa xe lâu.",
                        Status = "Replied",
                        Rating = 3,
                        InternalNote = "Đã bổ sung thêm cây nước nóng lạnh và bật điều hòa phòng chờ.",
                        CreatedAt = now.AddDays(-15),
                        UpdatedAt = now.AddDays(-14)
                    }
                };
                context.Contacts.AddRange(contacts);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                foreach (var c in contacts.Where(x => x.Status == "Replied" || x.Status == "Closed"))
                {
                    var reply = new ContactReply
                    {
                        ContactId = c.Id,
                        Message =
                            $"Chào anh/chị {c.FullName}, chúng tôi đã nhận được thông tin và xin phản hồi như sau: [Nội dung phản hồi từ Admin/CSKH]. Cảm ơn anh/chị đã đóng góp ý kiến để hoàn thiện dịch vụ.",
                        RepliedById = adminId,
                        IsInternal = false,
                        CreatedAt = c.CreatedAt.GetValueOrDefault(DateTimeOffset.UtcNow).AddHours(random.Next(1, 10)),
                        UpdatedAt = c.CreatedAt.GetValueOrDefault(DateTimeOffset.UtcNow).AddHours(random.Next(1, 10))
                    };
                    context.ContactReplies.Add(reply);
                }
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            // Seed Invoices
            if (!await context.Invoices.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var salesUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "tran.thi.b@anhemmotor.com", cancellationToken).ConfigureAwait(false);
                var userId = salesUser?.Id ?? context.Users.First().Id;

                var invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNumber = "HD-20260701-SH150",
                        IssueDate = DateTime.Now.AddDays(-8),
                        CustomerName = "Nguyễn Văn Hùng",
                        CustomerIdCard = "031203004567",
                        CustomerPhone = "0987654321",
                        CustomerAddress = "123 Giải Phóng, Hà Nội",
                        VehicleModel = "Honda SH 150i ABS",
                        VehicleColor = "Đen bóng",
                        ChassisNo = "RLHSH150I2026001",
                        EngineNo = "KF12E-1002345",
                        VehiclePrice = 102000000,
                        RegistrationFee = 5000000,
                        InsuranceFee = 1500000,
                        TotalAmount = 108500000,
                        PaymentMethod = "transfer",
                        BankName = "Vietcombank",
                        Status = "completed",
                        SalesPerson = "Trần Thị B",
                        DeliveryDate = DateTime.Now.AddDays(-6),
                        UserId = userId,
                        CreatedAt = DateTimeOffset.Now.AddDays(-8)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "HD-20260703-W110",
                        IssueDate = DateTime.Now.AddDays(-5),
                        CustomerName = "Lê Thị Thảo",
                        CustomerIdCard = "031203009999",
                        CustomerPhone = "0912345678",
                        CustomerAddress = "45 Cầu Giấy, Hà Nội",
                        VehicleModel = "Honda Wave Alpha 110cc",
                        VehicleColor = "Đỏ thẫm",
                        ChassisNo = "RLHWAVE110202602",
                        EngineNo = "HC11E-2003456",
                        VehiclePrice = 18500000,
                        RegistrationFee = 1500000,
                        InsuranceFee = 500000,
                        TotalAmount = 20500000,
                        PaymentMethod = "cash",
                        BankName = "",
                        Status = "completed",
                        SalesPerson = "Nguyễn Văn A",
                        DeliveryDate = DateTime.Now.AddDays(-5),
                        UserId = userId,
                        CreatedAt = DateTimeOffset.Now.AddDays(-5)
                    },
                    new Invoice
                    {
                        InvoiceNumber = "HD-20260705-V125",
                        IssueDate = DateTime.Now.AddDays(-2),
                        CustomerName = "Phạm Minh Đức",
                        CustomerIdCard = "031203008888",
                        CustomerPhone = "0904567890",
                        CustomerAddress = "78 Lê Lợi, Hải Phòng",
                        VehicleModel = "Honda Vision 125cc",
                        VehicleColor = "Xanh xi măng",
                        ChassisNo = "RLHVISION202603",
                        EngineNo = "JF58E-3004567",
                        VehiclePrice = 33000000,
                        RegistrationFee = 2500000,
                        InsuranceFee = 800000,
                        TotalAmount = 36300000,
                        PaymentMethod = "installment",
                        BankName = "",
                        Status = "pending",
                        SalesPerson = "Phạm Thị D",
                        DeliveryDate = DateTime.Now.AddDays(3),
                        UserId = userId,
                        CreatedAt = DateTimeOffset.Now.AddDays(-2)
                    }
                };

                context.Invoices.AddRange(invoices);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
