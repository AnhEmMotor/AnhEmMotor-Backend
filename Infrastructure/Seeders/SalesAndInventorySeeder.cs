using Domain.Constants.Order;
using Domain.Entities;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Seeders
{
    public static class SalesAndInventorySeeder
    {
        public static async Task SeedAsync(ApplicationDBContext context, CancellationToken cancellationToken)
        {
            // 1. Fetch product variants and colors
            var variants = await context.ProductVariants
                .Include(v => v.ProductVariantColors)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (variants.Count == 0)
                return;

            // 2. Check if we already have outputs or inputs
            if (await context.OutputOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var today = now.Date;

            // Fetch first active supplier
            var supplier = await context.Suppliers.FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
            var supplierId = supplier?.Id;

            // ==== A. SEED INVENTORY RECEIPTS (INPUTS) ====
            // Commented out due to schema changes
            // ...

            // ==== B. SEED SALES ORDERS (OUTPUTS) ====
            var random = new Random(42);
            var statuses = new[] {
                OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed,
                OrderStatus.Delivering, OrderStatus.WaitingPickup, OrderStatus.Pending,
                OrderStatus.Cancelled, OrderStatus.WaitingDeposit
            };

            var salesUsers = await context.Users
                .Where(u => u.Email == "nguyen.van.a@anhemmotor.com" || u.Email == "tran.thi.b@anhemmotor.com" || u.Email == "pham.thi.d@anhemmotor.com")
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var seededOutputs = new List<Output>();

            // 1. Seed monthly orders for the past 12 months to build historical charts
            for (int i = 11; i >= 0; i--)
            {
                var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(-i);
                int ordersInMonth = random.Next(10, 25); // 10 to 25 orders per month for nice charts

                for (int o = 0; o < ordersInMonth; o++)
                {
                    var orderDate = monthStart.AddDays(random.Next(1, 28)).AddHours(random.Next(8, 20));
                    var status = OrderStatus.Completed;
                    if (i == 0) // current month can have active/pending/cancelled orders
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

            // Add details (OutputInfo) for the orders
            foreach (var output in seededOutputs)
            {
                var orderDate = output.CreatedAt ?? now;
                var variant = variants[random.Next(variants.Count)];
                var color = variant.ProductVariantColors.FirstOrDefault();
                var qty = random.Next(1, 3); // 1 or 2 bikes per sale
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

            // ==== C. SEED REPAIR ORDERS (WORKSHOP TICKETS) ====
            if (!await context.RepairOrders.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var technicians = await context.EmployeeProfiles.ToListAsync(cancellationToken).ConfigureAwait(false);

                // 1. InProgress tickets
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

                // 2. Completed tickets in the past 30 days
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

                // 3. Overdue tickets
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
                        ExpectedCompletionTime = now.AddHours(-2), // overdue
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

            // ==== D. SEED CONTACTS & REPLIES (CUSTOMER CARE) ====
            if (!await context.Contacts.AnyAsync(cancellationToken).ConfigureAwait(false))
            {
                var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@anhem.com", cancellationToken).ConfigureAwait(false);
                var adminId = adminUser?.Id;

                var contacts = new List<Contact>
                {
                    new Contact
                    {
                        FullName = "Nguyễn Hoàng Nam",
                        Email = "nam.nguyen12@gmail.com",
                        PhoneNumber = "0948123456",
                        Subject = "Hỏi về chế độ bảo hành xe Honda SH",
                        Message = "Tôi mới mua xe SH ở showroom tuần trước, cho hỏi chế độ bảo hành xe định kỳ như thế nào?",
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
                        Message = "Nhân viên kỹ thuật lúc bảo dưỡng xe Winner X của tôi thái độ rất không hợp tác, không chịu kiểm tra kỹ xích tải.",
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
                        Message = "Tôi muốn mua trả góp xe Vision bản đặc biệt, cần làm những thủ tục gì và trả trước bao nhiêu?",
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

                // Add replies for Replied/Closed contacts
                foreach (var c in contacts.Where(x => x.Status == "Replied" || x.Status == "Closed"))
                {
                    var reply = new ContactReply
                    {
                        ContactId = c.Id,
                        Message = $"Chào anh/chị {c.FullName}, chúng tôi đã nhận được thông tin và xin phản hồi như sau: [Nội dung phản hồi từ Admin/CSKH]. Cảm ơn anh/chị đã đóng góp ý kiến để hoàn thiện dịch vụ.",
                        RepliedById = adminId,
                        IsInternal = false,
                        CreatedAt = c.CreatedAt.GetValueOrDefault(DateTimeOffset.UtcNow).AddHours(random.Next(1, 10)),
                        UpdatedAt = c.CreatedAt.GetValueOrDefault(DateTimeOffset.UtcNow).AddHours(random.Next(1, 10))
                    };
                    context.ContactReplies.Add(reply);
                }
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
