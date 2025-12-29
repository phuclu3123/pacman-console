using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text;
using System.Globalization;

namespace CinemaTicket
{
    class Customer
    {
        public string Name;   // Tên khách
        public string PhoneLast4; // 4 số cuối điện thoại
        public int Row;       // Hàng ghế
        public int Col;       // Cột ghế
        public double Price;  // Giá vé
    }
    enum SeatStatus
    {
        Empty = 0,    // Ghế trống
        Booked = 1,   // Ghế đã đặt
        Reserved = 2  // Ghế tạm giữ
    }
    class Program
    {
        const int ROWS = 10;
        const int COLS = 20;
        const double TICKET_PRICE = 50000;

        static SeatStatus[,] seats = new SeatStatus[ROWS, COLS];
        static List<Customer> customers = new List<Customer>();
        static int soldSeats = 0;
        static double revenue = 0;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            LoadCustomers();
            ShowWelcome();

            bool isRunning = true; // biến bool điều khiển chương trình

            while (isRunning)
            {
                ShowMenu();
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nNhập lựa chọn: ");
                if (!int.TryParse(Console.ReadLine(), out int choice)) //nếu nhập đúng thì lưu vào choice, nếu sai thì báo lỗi
                {
                    Console.WriteLine("Vui lòng nhập số!");
                    continue;
                }

                switch (choice)
                {
                    case 1: ShowSeats(); break;
                    case 2: BookTicket(); break;
                    case 3: CancelTicket(); break;
                    case 4: ShowStatistic(); break;
                    case 5: ShowHistory(); break;
                    case 6: SearchTicketByName(); break;
                    case 7: SortCustomersByName(); break;
                    case 8: EditTicket(); break;
                    case 0:
                        Console.WriteLine("Thoát chương trình...");
                        SaveCustomers();
                        isRunning = false; // dừng vòng lặp thay vì while(choice != 0)
                        break;
                    default:
                        Console.WriteLine("Lựa chọn không hợp lệ.");
                        break;
                }
            }
        }

        // ====== GIAO DIỆN ======
        static void ShowWelcome()
        {
            SnowEffect(4000, 70, 20); // Hiệu ứng tuyết rơi
            Console.ForegroundColor = ConsoleColor.Cyan; // Màu xanh dương cho tiêu đề
            Console.WriteLine(@"
██╗    ██╗ ███████╗ ██╗  ██╗    ███████╗ ██████╗ ███████╗███████╗
██║    ██║ ██╔════╝ ██║  ██║    ╚════██║██╔═══██╗╚════██║██╔════╝
██║    ██║ █████╗   ███████║    ███████║██║   ██║███████║███████╗
██║    ██║ ██╔══╝   ██║  ██║    ██╔════╝██║   ██║██╔════╝╚════██║
 ╚█████╔═╝ ███████╗ ██║  ██║    ███████╗╚██████╔╝███████╗███████║
  ╚════╝   ╚══════╝ ╚═╝  ╚═╝    ╚══════╝ ╚═════╝ ╚══════╝╚══════╝
");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(@"
 ██████╗██╗███╗   ██╗███████╗███╗   ███╗ █████╗ 
██╔════╝██║████╗  ██║██╔════╝████╗ ████║██╔══██╗
██║     ██║██╔██╗ ██║█████╗  ██╔████╔██║███████║
██║     ██║██║╚██╗██║██╔══╝  ██║╚██╔╝██║██╔══██║
╚██████╗██║██║ ╚████║███████╗██║ ╚═╝ ██║██║  ██║
 ╚═════╝╚═╝╚═╝  ╚═══╝╚══════╝╚═╝     ╚═╝╚═╝  ╚═╝
");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
 ███╗   ██╗██╗  ██╗ ██████╗ ███╗   ███╗     ██╗
 ████╗  ██║██║  ██║██╔═══██╗████╗ ████║     ██║
 ██╔██╗ ██║███████║██║   ██║██╔████╔██║     ██║
 ██║╚██╗██║██╔══██║██║   ██║██║╚██╔╝██║     ██║
 ██║ ╚████║██║  ██║╚██████╔╝██║ ╚═╝ ██║     ██║ 
 ╚═╝  ╚═══╝╚═╝  ╚═╝ ╚═════╝ ╚═╝     ╚═╝     ╚═╝
");
            Console.ResetColor(); // Đặt lại màu mặc định để mấy phần sau không bị ảnh hưởng
            Console.ForegroundColor = ConsoleColor.Yellow; // Màu vàng cho dòng phụ đề
            Console.WriteLine("         🎬 CINEMA TICKET MANAGEMENT 🎬");
            Console.ResetColor(); // Đặt lại màu mặc định
            Thread.Sleep(2000); // Tạm dừng 1 giây để người dùng kịp nhìn
            SmoothClear(); //thực hiện hàm SmoothClear
        }

        static void DrawHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            string line = new string('═', title.Length + 6); // +6 để có khoảng trống hai bên
            Console.WriteLine($"╔{line}╗"); // Vẽ đường viền trên
            Console.WriteLine($"║   {title}   ║");  // Vẽ tiêu đề ở giữa
            Console.WriteLine($"╚{line}╝");  // Vẽ đường viền dưới
            Console.ResetColor();
        }

        static void SmoothClear()
        {
            for (int i = 0; i < 3; i++) // Hiệu ứng chấm chấm
            {
                Console.Write(".");
                Thread.Sleep(500); //in ra mỗi dấu chấm là thêm 500ms
            }
            Console.Clear(); //xóa màn hình
        }

        static void ShowMenu() //Giao diện menu
        {
            Console.Clear();
            DrawHeader("MENU QUẢN LÝ VÉ RẠP"); // Vẽ tiêu đề menu
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("1. Hiển thị sơ đồ ghế");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("2. Đặt vé");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("3. Hủy vé");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("4. Thống kê");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("5. Xem lịch sử đặt vé");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("6. Tìm vé theo tên khách");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("7. Sắp xếp danh sách khách theo tên (A → Z)");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("8. Sửa vé khách hàng");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("0. Thoát");
            Console.ResetColor();
            Console.WriteLine("\n───────────────────────────────────────");
        }

        // ====== HIỂN THỊ GHẾ ======
        static void ShowSeats(bool wait = true)
        {
            Console.Clear();
            DrawHeader("SƠ ĐỒ GHẾ RẠP");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\n                       🎥  MÀN HÌNH  🎥");
            Console.ResetColor();

            Console.Write("     ");
            for (int j = 0; j < COLS; j++)
                Console.Write("{0,3}", j + 1);
            Console.WriteLine();

            for (int i = 0; i < ROWS; i++)
            {
                Console.Write($" {GetRowLetter(i),2}: ");
                for (int j = 0; j < COLS; j++)
                {
                    if (seats[i, j] == SeatStatus.Empty)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" ☐ ");
                    }
                    else if (seats[i, j] == SeatStatus.Booked)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(" ☒ ");
                    }

                }
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.WriteLine("\nChú thích: ☐ = Ghế trống, ☒ = Ghế đã đặt");
            if (wait) WaitAndClear();
        }
        static void ShowSeatsOnly()
        {
            ShowSeats(false);
        }


        // ====== ĐẶT VÉ ======
        // ====== ĐÃ SỬA LẠI HÀM NÀY ======
        static void BookTicket()
        {
            ShowSeatsOnly();
            Console.WriteLine("\n=== ĐẶT VÉ ===");

            // [VALIDATION QUAN TRỌNG] Tính số ghế còn trống
            int emptySeats = (ROWS * COLS) - soldSeats;
            
            // Nếu rạp đã đầy thì báo luôn, thoát hàm
            if (emptySeats == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ Rạp đã hết sạch vé! Không thể đặt thêm.");
                Console.ResetColor();
                WaitAndClear();
                return;
            }

            // 1. Hỏi hình thức mua (Chỉ để hiển thị thông báo cho hợp lý)
            Console.Write("Bạn đã đặt vé online chưa? (y/n): ");
            string online = Console.ReadLine().ToLower();

            if (online == "y")
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n[ONLINE] Vui lòng nhập thông tin đã đặt để xác nhận lấy vé:");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[TRỰC TIẾP] Nhập thông tin khách hàng mua vé mới:");
                Console.ResetColor();
            }

            // 2. Nhập Tên (Dùng chung cho cả Online và Trực tiếp)
            string name;
            while (true)
            {
                Console.Write("Nhập tên khách: ");
                name = Console.ReadLine()?.Trim(); 

                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("❌ Tên không được để trống!");
                }
                else if (name.Contains("|"))
                {
                    Console.WriteLine("❌ Tên không được chứa ký tự '|'!");
                }
                else
                {
                    break; // Tên hợp lệ
                }
            }

            // 3. Nhập SĐT (Dùng chung)
            string phoneLast4;
            while (true)
            {
                Console.Write("Nhập 4 số cuối điện thoại: ");
                phoneLast4 = Console.ReadLine();

                // Check độ dài và check từng ký tự có phải là số không
                bool isAllDigits = true;
                foreach (char c in phoneLast4)
                {
                    if (!char.IsDigit(c)) 
                    {
                        isAllDigits = false; 
                        break; 
                    }
                }

                if (phoneLast4.Length == 4 && isAllDigits)
                {
                    break; // Hợp lệ
                }
                Console.WriteLine("❌ Vui lòng nhập đúng 4 chữ số (0-9)!");
            }

            // 4. Nhập số lượng vé (Dùng chung)
            // [UPDATE] Đồng nhất hiển thị tiền tệ
            Console.WriteLine($"\nGiá vé niêm yết: {TICKET_PRICE:N0} VND");
            
            Console.Write($"Số lượng vé cần lấy (còn {emptySeats} chỗ): ");
            if (!int.TryParse(Console.ReadLine(), out int soVe) || soVe <= 0)
            {
                Console.WriteLine("❌ Số lượng phải là số dương!"); 
                WaitAndClear();
                return;
            }

            // Kiểm tra số lượng vé hợp lệ với số ghế trống
            if (soVe > emptySeats)
            {
                Console.WriteLine($"❌ Chỉ còn lại {emptySeats} ghế trống. Không thể đặt {soVe} vé!");
                WaitAndClear();
                return;
            }

            // 5. Tiến hành chọn ghế (Gọi hàm có tham số cho cả 2 trường hợp)
            for (int i = 0; i < soVe; i++)
            {
                Console.WriteLine($"\n>> Chọn ghế cho vé thứ {i + 1}:");
                BookSingleTicket(name, phoneLast4); 
            }

            WaitAndClear();
        }



        static void BookSingleTicket(string name, string phoneLast4)
        {
            Console.Write("Nhập hàng ghế (A-{0}): ", GetRowLetter(ROWS - 1));
            string rowInput = Console.ReadLine(); 

            if (string.IsNullOrEmpty(rowInput))
            {
                Console.WriteLine("❌ Vui lòng nhập ký tự hàng ghế!");
                return;
            }

            int row = GetRowIndexFromLetter(rowInput); // Hàm này đã có sẵn logic xử lý chữ hoa/thường
            
            if (row < 0 || row >= ROWS)
            {
                Console.WriteLine("❌ Hàng không hợp lệ!");
                return;
            }

            Console.Write("Nhập số cột (1-{0}): ", COLS);
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 1 || col > COLS)
            {
                Console.WriteLine("❌ Cột không hợp lệ!");
                return;
            }
            col--; // Chuyển về index 0-based

            // Kiểm tra trùng thông tin khách
            if (customers.Exists(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                c.PhoneLast4 == phoneLast4 &&
                c.Row == row + 1 && c.Col == col + 1))
            {
                Console.WriteLine($"❌ Khách {name} ({phoneLast4}) đã đặt ghế {GetRowLetter(row)}{col + 1} rồi!");
                return;
            }

            if (seats[row, col] == SeatStatus.Booked)
            {
                Console.WriteLine("❌ Ghế đã có người khác đặt!");
                return;
            }

            seats[row, col] = SeatStatus.Booked;
            soldSeats++;
            revenue += TICKET_PRICE;

            Customer c = new Customer
            {
                Name = name,
                PhoneLast4 = phoneLast4,
                Row = row + 1,
                Col = col + 1,
                Price = TICKET_PRICE
            };
            customers.Add(c);

            // ĐỒNG NHẤT FORMAT HIỂN THỊ
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Đặt vé thành công: {name} - Ghế {GetRowLetter(row)}{col + 1}");
            Console.ResetColor();

            // ĐỒNG NHẤT FORMAT LOG FILE (A1, B2...)
            File.AppendAllText("history.txt",
            $"[{DateTime.Now}] ĐẶT VÉ: {name} ({phoneLast4}) - Ghế {GetRowLetter(row)}{col + 1} - Giá {c.Price:N0} VND\n");

            SaveCustomers();
        }

        // ====== HÀM NẠP CHỒNG: ĐẶT VÉ TỪ APP VÉ ONLINE ======
        

        // ====== HỦY VÉ ======
        static void CancelTicket()
        {
            ShowSeatsOnly();
            Console.WriteLine("\n=== HỦY VÉ ===");

            Console.Write("Nhập hàng ghế (A-{0}): ", GetRowLetter(ROWS - 1));
            string rowInput = Console.ReadLine().ToUpper();
            int row = GetRowIndexFromLetter(rowInput);
            if (row < 0 || row >= ROWS) { Console.WriteLine("❌ Hàng không hợp lệ!"); return; }

            Console.Write("Nhập số cột (1-{0}): ", COLS);
            if (!int.TryParse(Console.ReadLine(), out int col) || col < 1 || col > COLS) { Console.WriteLine("❌ Sai dữ liệu!"); return; }
            col--; 

            int index = customers.FindIndex(c => c.Row == row + 1 && c.Col == col + 1);
            if (index == -1)
            {
                Console.WriteLine($"❌ Không tìm thấy vé tại ghế {GetRowLetter(row)}{col + 1}!");
                return;
            }

            var customer = customers[index];
            seats[row, col] = SeatStatus.Empty;
            soldSeats--;
            revenue -= customer.Price;
            customers.RemoveAt(index);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Hủy vé thành công cho ghế {GetRowLetter(row)}{col + 1}.");
            Console.ResetColor();

            File.AppendAllText("history.txt",
            $"[{DateTime.Now}] HỦY VÉ: {customer.Name} ({customer.PhoneLast4}) - Ghế {GetRowLetter(customer.Row - 1)}{customer.Col}\n");

            SaveCustomers();
            WaitAndClear();
        }
        // ====== SỬA VÉ ======
        static void EditTicket()
        {
            Console.Clear();
            DrawHeader("SỬA THÔNG TIN VÉ");

            // [VALIDATION] Nhập tên để tìm
            string name;
            while (true)
            {
                Console.Write("Nhập tên khách cần sửa: ");
                name = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(name)) break;
                Console.WriteLine("❌ Tên không được để trống!");
            }

            // [VALIDATION] Nhập SĐT để tìm
            string phoneLast4;
            while (true)
            {
                Console.Write("Nhập 4 số cuối điện thoại: ");
                phoneLast4 = Console.ReadLine();
                bool isDigit = true;
                foreach (char c in phoneLast4) { if (!char.IsDigit(c)) isDigit = false; }
                if (phoneLast4.Length == 4 && isDigit) break;
                Console.WriteLine("❌ Vui lòng nhập đúng 4 chữ số!");
            }

            var matches = customers.FindAll(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && c.PhoneLast4 == phoneLast4);

            if (matches.Count == 0)
            {
                Console.WriteLine("❌ Không tìm thấy vé khớp với thông tin trên!");
                WaitAndClear();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n>> Tìm thấy {matches.Count} vé:");
            for (int i = 0; i < matches.Count; i++)
            {
                string rowLetter = GetRowLetter(matches[i].Row - 1);
                Console.WriteLine($"{i + 1}. Ghế {rowLetter}{matches[i].Col} - Giá {matches[i].Price:N0} VND");
            }
            Console.ResetColor();

            Console.Write("\nChọn vé cần sửa (nhập số thứ tự): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 1 || choice > matches.Count)
            {
                Console.WriteLine("❌ Lựa chọn không hợp lệ!");
                WaitAndClear();
                return;
            }

            Customer targetTicket = matches[choice - 1]; 
            int index = customers.IndexOf(targetTicket); 

            // Lưu thông tin cũ
            string oldSeatName = $"{GetRowLetter(targetTicket.Row - 1)}{targetTicket.Col}";
            string oldName = targetTicket.Name;
            string oldPhone = targetTicket.PhoneLast4;

            Console.WriteLine("\nBạn muốn sửa gì?");
            Console.WriteLine("1. Đổi ghế");
            Console.WriteLine("2. Đổi tên khách");
            Console.WriteLine("3. Đổi 4 số điện thoại");
            Console.WriteLine("4. Hủy (thoát)");
            Console.Write("Lựa chọn: ");
            string option = Console.ReadLine();

            string logDetail = ""; 

            switch (option)
            {
                case "1":
                    ShowSeatsOnly();
                    
                    // [VALIDATION] Nhập hàng ghế mới
                    Console.Write("Nhập hàng ghế mới (A-{0}): ", GetRowLetter(ROWS - 1));
                    string rowInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(rowInput)) { Console.WriteLine("❌ Lỗi: Chưa nhập hàng ghế!"); WaitAndClear(); return; }
                    int newRow = GetRowIndexFromLetter(rowInput) + 1;
                    
                    if (newRow < 1 || newRow > ROWS) 
                    { 
                        Console.WriteLine("❌ Hàng không hợp lệ!"); 
                        WaitAndClear(); 
                        return; 
                    }

                    // [VALIDATION] Nhập cột ghế mới
                    Console.Write("Nhập cột ghế mới (1-{0}): ", COLS);
                    if (!int.TryParse(Console.ReadLine(), out int newCol) || newCol < 1 || newCol > COLS) 
                    { 
                        Console.WriteLine("❌ Cột không hợp lệ!"); 
                        WaitAndClear(); 
                        return; 
                    }

                    // [FIX LỖI HIỂN THỊ] Check trùng ghế
                    if (seats[newRow - 1, newCol - 1] == SeatStatus.Booked && 
                    (newRow != targetTicket.Row || newCol != targetTicket.Col)) 
                    {
                        Console.WriteLine("❌ Ghế này đã có người khác đặt!");
                        WaitAndClear(); // <--- QUAN TRỌNG: Dừng lại để khách đọc lỗi trước khi thoát
                        return;
                    }

                    // Update sơ đồ
                    seats[targetTicket.Row - 1, targetTicket.Col - 1] = SeatStatus.Empty;
                    seats[newRow - 1, newCol - 1] = SeatStatus.Booked;
                    
                    // Update data
                    customers[index].Row = newRow;
                    customers[index].Col = newCol;

                    string newSeatName = $"{GetRowLetter(newRow - 1)}{newCol}";
                    Console.WriteLine($"✅ Đã đổi từ ghế {oldSeatName} sang {newSeatName} thành công!");
                    logDetail = $"Đổi ghế: {oldSeatName} -> {newSeatName}";
                    break;

                case "2":
                    // [VALIDATION] Nhập tên mới
                    string newName;
                    while(true)
                    {
                        Console.Write("Nhập tên mới: ");
                        newName = Console.ReadLine()?.Trim();
                        if (!string.IsNullOrEmpty(newName) && !newName.Contains("|")) break;
                        Console.WriteLine("❌ Tên không hợp lệ (không để trống, không chứa '|')");
                    }
                    
                    customers[index].Name = newName;
                    Console.WriteLine($"✅ Đã cập nhật tên: {oldName} -> {newName}");
                    logDetail = $"Đổi tên: {oldName} -> {newName}";
                    break;

                case "3":
                    // [VALIDATION] Nhập SĐT mới
                    string newPhone;
                    while(true)
                    {
                        Console.Write("Nhập 4 số điện thoại mới: ");
                        newPhone = Console.ReadLine();
                        bool isDigitNew = true;
                        foreach(char c in newPhone) if(!char.IsDigit(c)) isDigitNew = false;
                        
                        if (newPhone.Length == 4 && isDigitNew) break;
                        Console.WriteLine("❌ Phải nhập đúng 4 chữ số!");
                    }

                    customers[index].PhoneLast4 = newPhone;
                    Console.WriteLine($"✅ Đã cập nhật SĐT: {oldPhone} -> {newPhone}");
                    logDetail = $"Đổi SĐT: {oldPhone} -> {newPhone}";
                    break;

                default:
                    Console.WriteLine("Đã hủy thao tác.");
                    WaitAndClear();
                    return;
            }

            // Ghi log
            File.AppendAllText("history.txt",
            $"[{DateTime.Now}] SỬA VÉ: {oldName} ({oldPhone}) - {logDetail}\n");

            SaveCustomers();
            WaitAndClear();
        }

        // ====== TÌM KIẾM KHÁCH HÀNG ======
        static bool FindCustomerByName(string name, out Customer found)
        {
            foreach (var c in customers)
            {
                if (c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    found = c; // gán dữ liệu cho biến out
                    return true; // tìm thấy
                }
            }

            found = new Customer(); // gán giá trị mặc định nếu không tìm thấy
            return false; // không tìm thấy
        }

        // ====== CHỨC NĂNG TÌM KIẾM VÉ ======
        static void SearchTicketByName()
        {
            Console.Clear();
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            DrawHeader("TÌM KIẾM VÉ THEO TÊN KHÁCH");
            Console.Write("\nNhập tên khách cần tìm: ");
            string searchName = Console.ReadLine();

            var matches = customers.FindAll(c => c.Name.Equals(searchName, StringComparison.OrdinalIgnoreCase));
            if (matches.Count == 0)
            {
                Console.WriteLine("\n❌ Không tìm thấy khách hàng này!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n>> Tìm thấy {matches.Count} kết quả:");
                Console.ResetColor();
                foreach (var r in matches)
                {
                    // Đồng nhất hiển thị dạng A1
                    string seatCode = $"{GetRowLetter(r.Row - 1)}{r.Col}";
                    Console.WriteLine($"- {r.Name,-20} | {r.PhoneLast4} | Ghế: {seatCode,-4} | {r.Price:N0} VND");
                }
            }
            WaitAndClear();
        }



        // ====== THỐNG KÊ ======
        //static void ShowStatistic()
        //{
        //    Console.Clear();
        //    DrawHeader("THỐNG KÊ RẠP");
        //    int totalSeats = ROWS * COLS; // Tổng số ghế
        //    int emptySeats = totalSeats - soldSeats; // Ghế trống
        //    double occupancy = (double)soldSeats / totalSeats * 100; // Tỷ lệ lấp đầy

        //    Console.WriteLine($"Tổng số ghế: {totalSeats}");
        //    Console.WriteLine($"Đã bán: {soldSeats}");
        //    Console.WriteLine($"Còn trống: {emptySeats}");
        //    Console.WriteLine($"Tỷ lệ lấp đầy: {occupancy:F2}%");
        //    Console.WriteLine($"Doanh thu: {revenue} VND");

        //    Console.WriteLine("\nTình trạng rạp:");
        //    Console.Write("Ghế đã bán:  ");
        //    DrawProgressBar(soldSeats, totalSeats, ConsoleColor.Red); // Thanh tiến độ cho ghế đã bán
        //    Console.Write("Ghế trống:   ");
        //    DrawProgressBar(emptySeats, totalSeats, ConsoleColor.Green); // Thanh tiến độ cho ghế trống

        //    WaitAndClear();
        //}
        static void ShowStatistic()
        {
            Console.Clear();
            DrawHeader("THỐNG KÊ RẠP");
            int totalSeats = ROWS * COLS; // Tổng số ghế
            int emptySeats = totalSeats - soldSeats; // Ghế trống
            double occupancy = (double)soldSeats / totalSeats * 100; // Tỷ lệ lấp đầy

            Console.WriteLine($"Tổng số ghế: {totalSeats}");
            Console.WriteLine($"Đã bán: {soldSeats}");
            Console.WriteLine($"Còn trống: {emptySeats}");
            Console.WriteLine($"Tỷ lệ lấp đầy: {occupancy:F2}%");
            Console.WriteLine($"Doanh thu: {revenue} VND");

            Console.WriteLine("\nTình trạng rạp:");
            Console.Write("Ghế đã bán:  ");
            DrawProgressBar(soldSeats, totalSeats, ConsoleColor.Red); // Thanh tiến độ cho ghế đã bán
            Console.Write("Ghế trống:   ");
            DrawProgressBar(emptySeats, totalSeats, ConsoleColor.Green); // Thanh tiến độ cho ghế trống

            WaitAndClear();
        }

        static void DrawProgressBar(int value, int total, ConsoleColor color) // Vẽ thanh tiến độ
        {
            int width = 30;
            int filled = (int)((double)value / total * width); // Tính số phần đã lấp đầy
            Console.ForegroundColor = color;
            Console.Write("[");
            Console.Write(new string('█', filled));
            Console.Write(new string(' ', width - filled));
            Console.WriteLine("]");
            Console.ResetColor();
        }

        // ====== LỊCH SỬ ======
        static void ShowHistory()
        {
            Console.Clear();
            DrawHeader("LỊCH SỬ ĐẶT / HỦY VÉ");
            if (!File.Exists("history.txt")) { Console.WriteLine("Chưa có lịch sử!"); WaitAndClear(); return; }
            string[] history = File.ReadAllLines("history.txt");
            if (history.Length == 0) Console.WriteLine("Lịch sử rỗng!");
            else foreach (string line in history) Console.WriteLine(line);
            WaitAndClear();
        }

        // ====== HIỂN THỊ GHẾ KHÔNG CHỜ ======
        

        // ====== LƯU / NẠP KHÁCH ======
        static void SaveCustomers()
        {
            try
            {
                // tạo bản sao .bak trước khi ghi đè
                if (File.Exists("customers.txt"))
                    File.Copy("customers.txt", "customers.bak", true);

                using (StreamWriter sw = new StreamWriter("customers.txt"))
                {
                    foreach (var c in customers)
                        sw.WriteLine($"{c.Name}|{c.PhoneLast4}|{c.Row}|{c.Col}|{c.Price}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu file: " + ex.Message);
            }
        }


        static void LoadCustomers()
        {
            try
            {
                if (!File.Exists("customers.txt")) return;
                string[] lines = File.ReadAllLines("customers.txt");
                customers.Clear();
                soldSeats = 0;
                revenue = 0;
                Array.Clear(seats, 0, seats.Length);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split('|');
                    if (parts.Length != 5) continue;

                    string name = parts[0];
                    string phone = parts[1];

                    if (!int.TryParse(parts[2], out int row)) continue;
                    if (!int.TryParse(parts[3], out int col)) continue;
                    if (!double.TryParse(parts[4], out double price)) continue;

                    // KIỂM TRA HỢP LỆ TRƯỚC KHI THÊM
                    if (row - 1 >= 0 && row - 1 < ROWS && col - 1 >= 0 && col - 1 < COLS)
                    {
                        Customer c = new Customer
                        {
                            Name = name,
                            PhoneLast4 = phone,
                            Row = row,
                            Col = col,
                            Price = price
                        };
                        customers.Add(c);

                        seats[row - 1, col - 1] = SeatStatus.Booked;
                        soldSeats++;
                        revenue += c.Price;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi nạp dữ liệu: " + ex.Message);
            }
        }


        static void WaitAndClear()
        {
            Console.WriteLine("\nNhấn Enter để quay về menu...");
            Console.ReadLine();
            SmoothClear();
        }
        static string GetFirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            
            // Trim và Split loại bỏ các khoảng trắng thừa ở giữa
            string[] parts = fullName.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length > 0)
                return parts[parts.Length - 1]; // Lấy từ cuối cùng
                
            return fullName;
        }
        // hàm phụ trợ so sánh tên theo nước mình (logic nc ngoài là so sánh trái sang phải)
        static void SortCustomersByName()
        {
            if (customers.Count == 0)
            {
                Console.WriteLine("Chưa có khách nào để sắp xếp!");
                WaitAndClear();
                return;
            }

            // Khởi tạo bộ so sánh tiếng Việt
            CultureInfo viVn = new CultureInfo("vi-VN");

            customers.Sort((a, b) =>
            {
                string nameA = GetFirstName(a.Name);
                string nameB = GetFirstName(b.Name);

                // 1. So sánh Tên trước theo chuẩn tiếng Việt
                int result = string.Compare(nameA, nameB, true, viVn);

                // 2. Nếu tên trùng, so sánh cả Họ Tên đầy đủ
                if (result == 0)
                {
                    return string.Compare(a.Name, b.Name, true, viVn);
                }

                return result;
            });

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n>> Danh sách khách sau khi sắp xếp theo tên (A → Z):\n");
            Console.ResetColor();

            foreach (var c in customers)
            {
                string rowLetter = GetRowLetter(c.Row - 1);
                string seatCode = $"{rowLetter}{c.Col}";
                Console.WriteLine($"- {c.Name,-25} | {c.PhoneLast4} | Ghế: {seatCode,-4} | {c.Price:N0} VND");
            }

            WaitAndClear();
        }
        static string GetRowLetter(int rowIndex)
        {
            // rowIndex = 0-based (hàng 0 -> 'A', hàng 1 -> 'B', ...)
            return ((char)('A' + rowIndex )).ToString();
        }
        static int GetRowIndexFromLetter(string letter)
        {
            if (string.IsNullOrEmpty(letter)) return -1;
            char c = char.ToUpper(letter[0]);
            return c - 'A'; // A→0, B→1, C→2, ...
        }


        // ====== HIỆU ỨNG TUYẾT RƠI ======
        static void SnowEffect(int durationMs = 4000, int width = 10, int height = 10)
        {
            Console.Clear();
            Random rnd = new Random();
            DateTime endTime = DateTime.Now.AddMilliseconds(durationMs);
            char[] flakes = { '*', '.', '❄', '❅' };
            List<(int x, int y, char c)> snow = new List<(int, int, char)>();

            while (DateTime.Now < endTime)
            {
                // Tạo hạt tuyết mới ngẫu nhiên
                if (snow.Count < 80)
                    snow.Add((rnd.Next(0, width), 0, flakes[rnd.Next(flakes.Length)]));

                // Vẽ lại toàn màn hình
                Console.SetCursorPosition(0, 0);
                char[,] screen = new char[height, width];
                foreach (var s in snow)
                {
                    if (s.y < height && s.x < width)
                        screen[s.y, s.x] = s.c;
                }

                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                        Console.Write(screen[i, j] == '\0' ? ' ' : screen[i, j]);
                    Console.WriteLine();
                }

                // Cho tuyết rơi xuống
                for (int i = 0; i < snow.Count; i++)
                {
                    var s = snow[i];
                    s.y++;
                    if (s.y >= height)
                        snow[i] = (rnd.Next(0, width), 0, flakes[rnd.Next(flakes.Length)]);
                    else
                        snow[i] = s;
                }

                Thread.Sleep(100);
            }

            Console.Clear();
        }


    }
}
