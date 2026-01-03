using BO;

namespace BlTest;

internal class Program
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    static void Main(string[] args)
    {
        try
        {
            MainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Runs the main menu loop.
    /// </summary>
    private static void MainMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Main Menu:");
            Console.WriteLine("0 - Exit");
            Console.WriteLine("1 - Courier Management");
            Console.WriteLine("2 - Order Management");
            Console.WriteLine("3 - Admin Management");
            Console.WriteLine("4 - Print All Data");
            Console.WriteLine();

            try
            {
                Console.Write("Choose an option: ");
                if (!int.TryParse(Console.ReadLine(), out int choice)) throw new FormatException("Invalid input!");

                switch (choice)
                {
                    case 0:
                        return;
                    case 1:
                        ManageCouriers();
                        break;
                    case 2:
                        ManageOrders();
                        break;
                    case 3:
                        ManageAdmin();
                        break;
                    case 4:
                        PrintAllData();
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (BO.BlException ex)
            {
                PrintBlException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    #region Courier Menu

    /// <summary>
    /// Manages the BO operations for Couriers.
    /// </summary>
    private static void ManageCouriers()
    {
        while (true)
        {
            Console.WriteLine("\nCourier Management Menu:");
            Console.WriteLine("0 - Back");
            Console.WriteLine("1 - Create Courier");
            Console.WriteLine("2 - Read Courier by ID");
            Console.WriteLine("3 - List All Couriers");
            Console.WriteLine("4 - Update Courier");
            Console.WriteLine("5 - Delete Courier");
            Console.WriteLine("6 - Login (username/password)");
            Console.WriteLine();

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input!");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;

                    case 1: // Add Courier
                        Console.Write("Enter requester (manager) ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int addReqId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        BO.Courier newCourier = GetCourierInput(-1);
                        s_bl.Courier.AddCourier(newCourier);
                        Console.WriteLine("Courier created successfully.");
                        break;

                    case 2: // Read Courier details
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int readReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Courier ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int courierId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var courier = s_bl.Courier.GetCourierDetails(courierId);
                        Console.WriteLine(courier);
                        break;

                    case 3: // List all Couriers
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int listReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var allCouriers = s_bl.Courier.GetCouriersList(listReq);
                        foreach (var c in allCouriers)
                            Console.WriteLine(c);
                        break;

                    case 4: // Update Courier
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int updReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Courier ID to update: ");
                        if (!int.TryParse(Console.ReadLine(), out int updateId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        // show current (optional)
                        var current = s_bl.Courier.GetCourierDetails(updateId);
                        Console.WriteLine(current);

                        var updatedCourier = GetCourierInput(updateId);
                        s_bl.Courier.UpdateCourier(updReq, updatedCourier);
                        Console.WriteLine("Courier updated successfully.");
                        break;

                    case 5: // Delete Courier
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int delReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Courier ID to delete: ");
                        if (!int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        s_bl.Courier.DeleteCourier(delReq, deleteId);
                        Console.WriteLine("Courier deleted successfully.");
                        break;

                    case 6: // Login
                        Console.Write("Enter username: ");
                        string username = Console.ReadLine() ?? "";
                        Console.Write("Enter password: ");
                        string password = Console.ReadLine() ?? "";

                        var role = s_bl.Courier.Login(username, password);
                        Console.WriteLine($"Role: {role}");
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (BO.BlException ex)
            {
                PrintBlException(ex);
            }
        }
    }

    /// <summary>
    /// Gets input from the user to create or update a courier.
    /// Validates format using TryParse / regex where appropriate, but logical validations are done in BL.
    /// </summary>
    /// <param name="id">Courier ID. If -1, a new ID will be requested from user.</param>
    /// <returns>A new or updated BO.Courier object.</returns>
    private static BO.Courier GetCourierInput(int id)
    {
        if (id == -1)
        {
            Console.Write("Enter Courier ID: ");
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.WriteLine("Invalid input! Courier ID must be an integer.");
        }

        Console.Write("Enter Full Name: ");
        string fullName;
        while (string.IsNullOrWhiteSpace(fullName = Console.ReadLine()))
            Console.WriteLine("Full Name cannot be empty. Please try again.");

        Console.Write("Enter Phone (10 digits, starts with 0): ");
        string phone;
        while (true)
        {
            phone = Console.ReadLine() ?? "";
            if (System.Text.RegularExpressions.Regex.IsMatch(phone, @"^0\d{9}$")) break;
            Console.WriteLine("Invalid phone number format! Must be 10 digits and start with 0.");
        }

        Console.Write("Enter Email: ");
        string email;
        while (true)
        {
            email = Console.ReadLine() ?? "";
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address == email) break;
            }
            catch
            {
            }

            Console.WriteLine("Invalid email format! Please try again.");
        }

        Console.Write("Enter Password (optional, will be defaulted if empty): ");
        string? password = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(password)) password = "Default1A!";

        Console.Write("Is Active (true/false, default true): ");
        string isActiveInput = Console.ReadLine();
        bool isActive = string.IsNullOrWhiteSpace(isActiveInput)
            ? true
            : bool.TryParse(isActiveInput, out bool tmpA) && tmpA;

        Console.Write("Enter Max Distance (optional, in km): ");
        string? maxDistanceInput = Console.ReadLine();
        double? maxDistance = null;
        if (!string.IsNullOrWhiteSpace(maxDistanceInput) && double.TryParse(maxDistanceInput, out double md))
            maxDistance = md;

        Console.Write("Enter Shipment Type (Walk, Bicycle, Motorcycle, Car) - default Motorcycle: ");
        string shipmentTypeInput = Console.ReadLine();
        ShipmentType shipmentType = ShipmentType.Motorcycle;
        if (!string.IsNullOrWhiteSpace(shipmentTypeInput))
        {
            if (!Enum.TryParse(typeof(ShipmentType), shipmentTypeInput, true, out var st))
                Console.WriteLine("Unknown shipment type, defaulting to Motorcycle.");
            else
                shipmentType = (ShipmentType)st;
        }

        return new BO.Courier
        {
            Id = id,
            FullName = fullName,
            PhoneNumber = phone,
            Email = email,
            Password = password,
            IsActive = isActive,
            MaxDistance = maxDistance,
            ShipmentType = shipmentType,
            StartWorkTime = DateTime.Now,
            TotalDeliveredLate = 0,
            TotalDeliveredOnTime = 0,
            OrderInProgress = null
        };
    }

    #endregion

    #region Orders Menu

    private static void ManageOrders()
    {
        while (true)
        {
            Console.WriteLine("\nOrder Management Menu:");
            Console.WriteLine("0 - Back");
            Console.WriteLine("1 - Create Order");
            Console.WriteLine("2 - Read Order by ID");
            Console.WriteLine("3 - List All Orders");
            Console.WriteLine("4 - Update Order");
            Console.WriteLine("5 - Delete Order");
            Console.WriteLine("6 - Cancel Order");
            Console.WriteLine("7 - Assign Order to Courier");
            Console.WriteLine("8 - Complete Delivery (courier)");
            Console.WriteLine();

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input!");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;

                    case 1: // Create Order
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int addReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var newOrder = GetOrderInput(-1);
                        s_bl.Order.AddOrder(addReq, newOrder);
                        Console.WriteLine("Order created successfully.");
                        break;

                    case 2: // Read Order
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int readReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Order ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int id))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var order = s_bl.Order.GetOrderDetails(readReq, id);
                        Console.WriteLine(order);
                        break;

                    case 3: // List all Orders
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int listReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var allOrders = s_bl.Order.GetOrdersList(listReq);
                        foreach (var o in allOrders)
                            Console.WriteLine(o);
                        break;

                    case 4: // Update Order
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int updReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Order ID to update: ");
                        if (!int.TryParse(Console.ReadLine(), out int updateId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        var updatedOrder = GetOrderInput(updateId);
                        s_bl.Order.UpdateOrder(updReq, updatedOrder);
                        Console.WriteLine("Order updated successfully.");
                        break;

                    case 5: // Delete Order
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int delReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Order ID to delete: ");
                        if (!int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        s_bl.Order.DeleteOrder(delReq, deleteId);
                        Console.WriteLine("Order deleted successfully.");
                        break;

                    case 6: // Cancel Order
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int cancelReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Order ID to cancel: ");
                        if (!int.TryParse(Console.ReadLine(), out int cancelId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        s_bl.Order.CancelOrder(cancelReq, cancelId);
                        Console.WriteLine("Order cancelled successfully.");
                        break;

                    case 7: // Assign order to courier
                        Console.Write("Enter requester ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int assignReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Courier ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int courierId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Order ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int orderIdAssign))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        s_bl.Order.AssignOrderToCourier(assignReq, courierId, orderIdAssign);
                        Console.WriteLine("Order assigned to courier.");
                        break;

                    case 8: // Complete delivery
                        Console.Write("Enter requester (courier) ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int completeReq))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Courier ID (must match requester): ");
                        if (!int.TryParse(Console.ReadLine(), out int completeCourier))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        Console.Write("Enter Delivery ID: ");
                        if (!int.TryParse(Console.ReadLine(), out int deliveryId))
                        {
                            Console.WriteLine("Invalid input!");
                            break;
                        }

                        s_bl.Order.CompleteDelivery(completeReq, completeCourier, deliveryId);
                        Console.WriteLine("Delivery completed.");
                        break;

                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (BO.BlException ex)
            {
                PrintBlException(ex);
            }
        }
    }

    /// <summary>
    /// Gets input from user to create or update an order.
    /// Minimal fields captured here; BL performs logical validation.
    /// </summary>
    private static BO.Order GetOrderInput(int id)
    {
        Console.Write("Enter Order Type (e.g., General) - optional, press Enter to skip: ");
        string typeInput = Console.ReadLine();
        OrderType orderType = OrderType.General;
        if (!string.IsNullOrWhiteSpace(typeInput) && Enum.TryParse(typeof(OrderType), typeInput, true, out var ot))
            orderType = (OrderType)ot;

        Console.Write("Enter Description (optional): ");
        string? description = Console.ReadLine();

        Console.Write("Enter Full Address: ");
        string address = Console.ReadLine() ?? "";

        Console.Write("Enter Customer Full Name: ");
        string customerName = Console.ReadLine() ?? "";

        Console.Write("Enter Customer Phone: ");
        string customerPhone = Console.ReadLine() ?? "";

        return new BO.Order
        {
            Id = id,
            OrderType = orderType,
            Description = description,
            FullAddress = address,
            CustomerFullName = customerName,
            CustomerPhone = customerPhone,
            Latitude = 0,
            Longitude = 0,
            OrderOpenTime = DateTime.Now
        };
    }

    #endregion

    #region Admin Menu

    /// <summary>
    /// Manages the BO operations for the admin.
    /// </summary>
    private static void ManageAdmin()
    {
        while (true)
        {
            Console.WriteLine("\nAdmin Management:");
            Console.WriteLine("0 - Back");
            Console.WriteLine("1 - Reset database");
            Console.WriteLine("2 - Initialize database");
            Console.WriteLine("3 - Advance clock by unit");
            Console.WriteLine("4 - Display current clock");
            Console.WriteLine("5 - Get config");
            Console.WriteLine("6 - Set config");
            Console.WriteLine();

            Console.Write("Choose an option: ");
            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input!");
                continue;
            }

            try
            {
                switch (choice)
                {
                    case 0: return;
                    case 1:
                        s_bl.Admin.ResetDB();
                        Console.WriteLine("Database reset.");
                        break;
                    case 2:
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("Database initialized.");
                        break;
                    case 3:
                        Console.WriteLine("Choose a time unit to advance the clock:");
                        Console.WriteLine("1 - Minute");
                        Console.WriteLine("2 - Hour");
                        Console.WriteLine("3 - Day");
                        Console.WriteLine("4 - Month");
                        Console.WriteLine("5 - Year");
                        Console.Write("Your choice: ");
                        if (!int.TryParse(Console.ReadLine(), out int unitChoice) || unitChoice < 1 || unitChoice > 5)
                        {
                            Console.WriteLine("Invalid choice.");
                            break;
                        }

                        TimeUnit tu = (TimeUnit)(unitChoice - 1);
                        s_bl.Admin.ForwardClock(tu);
                        Console.WriteLine($"New clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 4:
                        Console.WriteLine($"Current clock: {s_bl.Admin.GetClock()}");
                        break;
                    case 5:
                        var cfg = s_bl.Admin.GetConfig();
                        Console.WriteLine("Config:");
                        Console.WriteLine(cfg);
                        break;
                    case 6:
                        Console.Write("Enter RiskRange (minutes, optional): ");
                        var hasRiskRange = int.TryParse(Console.ReadLine(), out var riskRange);

                        Console.Write("Enter Company Address (optional): ");
                        var compAddr = Console.ReadLine();
                        var hasCompAddr = !string.IsNullOrWhiteSpace(compAddr);

                        Console.Write("Enter Manager Password (optional): ");
                        var managerPwd = Console.ReadLine();
                        var hasManagerPwd = !string.IsNullOrWhiteSpace(managerPwd);

                        Console.Write("Enter Max Distance (optional): ");
                        var hasMaxDist = double.TryParse(Console.ReadLine(), out var maxDist);

                        Console.Write("Enter Average Car Speed (optional): ");
                        var hasAvgCar = double.TryParse(Console.ReadLine(), out var avgCar);

                        Console.Write("Enter Average Bike Speed (optional): ");
                        var hasAvgBike = double.TryParse(Console.ReadLine(), out var avgBike);

                        Console.Write("Enter Average Bicycle Speed (optional): ");
                        var hasAvgBicycle = double.TryParse(Console.ReadLine(), out var avgBicycle);

                        Console.Write("Enter Average Walk Speed (optional): ");
                        var hasAvgWalk = double.TryParse(Console.ReadLine(), out var avgWalk);

                        Console.Write("Enter Max Delivery Time (minutes, optional): ");
                        var hasMaxDel = int.TryParse(Console.ReadLine(), out var maxDel);

                        Console.Write("Enter Time To Be Dispose (minutes, optional): ");
                        var hasDispose = int.TryParse(Console.ReadLine(), out var dispose);

                        var newCfg = new BO.Config();

                        if (hasRiskRange) newCfg.RiskRange = TimeSpan.FromMinutes(riskRange);

                        if (hasManagerPwd) newCfg.ManagerPassword = managerPwd;
                        if (hasMaxDist) newCfg.MaxDistance = maxDist;
                        if (hasAvgCar) newCfg.AverageCarSpeed = avgCar;
                        if (hasAvgBike) newCfg.AverageBikeSpeedBike = avgBike;
                        if (hasAvgBicycle) newCfg.AverageBicycleSpeed = avgBicycle;
                        if (hasAvgWalk) newCfg.AverageWalkSpeed = avgWalk;
                        if (hasMaxDel) newCfg.MaxDeliveryTime = TimeSpan.FromMinutes(maxDel);
                        if (hasDispose) newCfg.TimeToBeDispose = TimeSpan.FromMinutes(dispose);
                        s_bl.Admin.SetConfig(newCfg);
                        Console.WriteLine("Config updated.");
                        break;
                    default:
                        Console.WriteLine("Invalid choice.");
                        break;
                }
            }
            catch (BO.BlException ex)
            {
                PrintBlException(ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }

    #endregion

    #region Helpers & Print

    /// <summary>
    /// Print BL exception details (type, message, inner).
    /// </summary>
    private static void PrintBlException(BO.BlException ex)
    {
        Console.WriteLine($"BL Exception: {ex.GetType().Name} - {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
    }

    /// <summary>
    /// Prints summary lists of couriers and orders.
    /// </summary>
    private static void PrintAllData()
    {
        Console.WriteLine("\nCouriers:");
        try
        {
            Console.Write("Enter requester ID to list couriers: ");
            if (!int.TryParse(Console.ReadLine(), out int req))
            {
                Console.WriteLine("Invalid input!");
                return;
            }

            var couriers = s_bl.Courier.GetCouriersList(req);
            if (couriers != null && couriers.Any())
            {
                foreach (var c in couriers) Console.WriteLine(c);
            }
            else
            {
                Console.WriteLine("No couriers found.");
            }
        }
        catch (BO.BlException ex)
        {
            PrintBlException(ex);
        }

        Console.WriteLine("\nOrders:");
        try
        {
            Console.Write("Enter requester ID to list orders: ");
            if (!int.TryParse(Console.ReadLine(), out int reqO))
            {
                Console.WriteLine("Invalid input!");
                return;
            }

            var orders = s_bl.Order.GetOrdersList(reqO);
            if (orders != null && orders.Any())
            {
                foreach (var o in orders) Console.WriteLine(o);
            }
            else
            {
                Console.WriteLine("No orders found.");
            }
        }
        catch (BO.BlException ex)
        {
            PrintBlException(ex);
        }
    }

    #endregion
}