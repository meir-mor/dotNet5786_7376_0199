namespace Dal;
using DalApi;
using DO;

public static class Initialization
{
    private static IConfig? s_dalConfig;
    private static IOrder? s_dalOrder;
    private static ICourier? s_dalCourier;
    private static IDelivery? s_dalDelivery;
    
    private static readonly Random s_rand = new();
    
    public static void Do(IConfig dalConfig, IOrder dalOrder, ICourier dalCourier, IDelivery dalDelivery)
    {
        s_dalConfig = dalConfig ?? throw new NullReferenceException("DAL object can not be null!");
        s_dalOrder = dalOrder ?? throw new NullReferenceException("DAL object can not be null!");
        s_dalCourier = dalCourier ?? throw new NullReferenceException("DAL object can not be null!");
        s_dalDelivery = dalDelivery ?? throw new NullReferenceException("DAL object can not be null!");
        
        Console.WriteLine("Reset Configuration values and List values...");
        s_dalConfig.Reset();
        s_dalOrder.DeleteAll();
        s_dalCourier.DeleteAll();
        s_dalDelivery.DeleteAll();
        
        Console.WriteLine("Initializing Students list ...");
        CreateOrder();
        CreateCourier();
        CreateDelivery();

    }

    private static void CreateOrder(int count = 50)
    {

        string[] addresses =
        {
            // Herzliya
            "Wingate 38 Herzliya",
            "Abba Eban 2 Herzliya",
            "Arlozorov 4 Herzliya",
            "Ben Yehuda 12 Herzliya",
            "Hagalil 11 Herzliya",
            "Maskit 27 Herzliya",
            "Ben Gurion 12 Herzliya",
            "Ramat Yam 60 Herzliya",
            "Weizmann 8 Kfar Saba",
            "Shenkar 3 Herzliya",

            // Netanya
            "Herzl 20 Netanya",
            "Ben Gurion 1 Netanya",
            "Haatzmaut 34 Sderot",
            "Tchernichovsky 7 Netanya",
            "Dizengoff 16 Netanya",
            "Sheshet Hayamim 3 Netanya",
            "Ruppin 21 Netanya",
            "HaNeviim 18 Netanya",
            "Ben Ami 28 Netanya",
            "Tchernichovsky 32 Netanya",

            // Ra'anana
            "Ahuza 124 Raanana",
            "HaPalmach 18 Raanana",
            "Weizmann 40 Kfar Saba",
            "Herzl 53 Raanana",
            "Nordau 5 Raanana",
            "HaRav Kook 10 Raanana",
            "HaYovel 16 Raanana",
            "Ben Gurion 5 Raanana",
            "HaEshel 7 Raanana",
            "Dizengoff 48 Tel Aviv",

            // Tel Aviv
            "Rothschild 22 Tel Aviv",
            "Ibn Gabirol 15 Tel Aviv",
            "Dizengoff 102 Tel Aviv",
            "Allenby 99 Tel Aviv",
            "King George 29 Tel Aviv",
            "Shenkin 17 Tel Aviv",
            "Bograshov 20 Tel Aviv",
            "Frishman 22 Tel Aviv",
            "Yehuda HaMaccabi 15 Tel Aviv",
            "Mikve Israel 19 Tel Aviv",

            // Kfar Saba
            "Weizmann 35 Kfar Saba",
            "Herzl 12 Kfar Saba",
            "Weizmann 50 Kfar Saba",
            "Arlozorov 25 Kfar Saba",
            "Dizengoff 55 Tel Aviv",
            "Herzl 78 Kfar Saba",
            "Tel Hai 33 Kfar Saba",
            "HaRav Maimon 6 Herzliya",
            "Rothschild 9 Kfar Saba",
            "Sokolov 3 Kfar Saba"
        };

        double[] latitudes = {
    32.1793471, 32.1607272, 32.1606701, 32.1627761, 32.1658838, 32.1632571,
    32.1650377, 32.171456, 32.1779805, 32.1602438, 32.3290299, 32.279103,
    31.5300071, 32.3219803, 32.3306203, 32.321458, 32.28087, 32.2967773,
    32.321458, 32.3208972, 32.1808257, 32.1905863, 32.1705469, 32.1775266,
    32.1854065, 32.1961516, 32.1843411, 32.180409, 32.1916612, 32.0748561,
    32.0627896, 32.0726935, 32.0796074, 32.0646173, 32.0717837, 32.0695395,
    32.0769951, 32.0797596, 32.0938841, 32.062857, 32.177421, 32.1709483,
    32.1765135, 32.1708769, 32.0754236, 32.1772724, 32.1737073, 32.1652867,
    32.1709569, 32.1762246
};

        double[] longitudes = {
    34.8058526, 34.8103495, 34.8428238, 34.8451422, 34.8439588, 34.8107107,
    34.8424487, 34.800546, 34.8957918, 34.8095785, 34.8555807, 34.852303,
    34.5897336, 34.8527596, 34.8543236, 34.853196, 34.853791, 34.8577569,
    34.853196, 34.8557568, 34.8739939, 34.8741786, 34.9286637, 34.8678441,
    34.8853162, 34.8743499, 34.8725648, 34.87231, 34.8585775, 34.7761884,
    34.7714756, 34.7816096, 34.7740421, 34.7726696, 34.7731051, 34.7723678,
    34.7692827, 34.7706442, 34.7860657, 34.7770402, 34.899857, 34.9104764,
    34.9015956, 34.9047851, 34.7748328, 34.9121208, 34.9138018, 34.83199,
    34.9070413, 34.9028277
};
        string[] customerName =
{
    "Daniel Levy", "Noa Friedman", "Eli Katz", "Maya Cohen", "Yonatan Mizrachi",
    "Sara Gold", "Ronen Shalev", "Hila Barak", "Omer Levi", "Tamar Peretz",
    "Amit Rosen", "Yaara Azulay", "Michal Golan", "Itay Sapir", "Lior Shimon",
    "Avital Ben-David", "Gal Noy", "Tzvika Mor", "Adi Malka", "Eden Levi",
    "Shani Alon", "David Cohen", "Ella Sapir", "Roi Bachar", "Rotem Menashe",
    "Tal Shapiro", "Ariel Ben-Ami", "Hadar Silver", "Ofir Gil", "Nofar Ben-Atar",
    "Yael Katz", "Shira Ben-David", "Michal Avraham", "Ronen Fisher", "Oded Tal",
    "Maayan Sasson", "Hanan Ravid", "Dana Biran", "Elad Rozen", "Linor Yaakov",
    "Benny Tal", "Gali Levy", "Tomer Brill", "Itamar Cohen", "Yael Ben-Zvi",
    "Sarah Cohen", "Erez Ben-Yosef", "Avital Ashkenazi", "Nir Carmi", "Gal Noy"
};
        for (var i = 0; i < count; i++)
        {
            s_dalOrder?.Create(new Order(
                Id: 0,
                Type: (OrderType)s_rand.Next(Enum.GetValues(typeof(OrderType)).Length), // random type
                Description: $"Order number {i} - autogenerated",
                Address: addresses[i % 50],
                CustomerName: customerName[i],
                CustomerPhone: $"050-{s_rand.Next(1000000, 9999999)}",
                IsFragile: s_rand.NextDouble() > 0.5,
                CreatedAt: s_dalConfig!.Clock.AddMinutes(-s_rand.Next(0, 24 * 60)).AddSeconds(-DateTime.Now.Second),
                //OrderStatus: OrderStatus.Open
                ));
        }
    }

    private static void CreateCourier(int count = 20)
    {
        string[] courierName =
            {
            "Ori Zigi", "Itay Blum", "Amit Grossman", "Tal Shapiro", "Yossi Mizrachi",
            "Shira Ben-David", "Sarah Cohen", "Michal Avraham", "Nir Carmi", "Gal Noy",
            "Ronen Fisher", "Yael Katz", "Hila Raz", "Noam Peretz", "Maya Solomon",
            "Erez Ben-Yosef", "Avital Ashkenazi", "Ariel Ben-Ami", "Tzvika Mor", "Lior Shani",
            "Adi Malka", "Doron Peled", "Eden Levi", "Benny Tal", "Gali Levy",
            "Avi Nave", "Yaara Shmuel", "Yonatan Green", "Hadar Silver", "Ella Sapir",
            "David Golan", "Shani Alon", "Omer Dahan", "Rotem Menashe", "Roi Bachar",
            "Tomer Brill", "Linor Yaakov", "Hodaya Shaked", "Elad Rozen", "Maayan Sasson",
            "Hanan Ravid", "Ron Aloni", "Galit Azulay", "Oded Tal", "Dana Biran",
            "Ofir Gil", "Nofar Ben-Atar", "Ruth Shalom", "Itamar Cohen", "Yael Ben-Zvi"
        };
        for (var i = 0; i < count; i++)
        {
            int id;
            do
                id= s_rand.Next(10000000, 99999999);
            while (s_dalCourier?.Read(id) != null);//כאן להוסיף תנאי חדש או שהערכים לא תקינים!!
           
            s_dalCourier?.Create(new Courier(
                Id: id,
                Name: courierName[i],
                Phone: $"052-{s_rand.Next(1000000, 9999999)}",
                Email: $"{courierName[i]}@gmail.com",
                Password: "0",
                IsActive: s_rand.NextDouble() > 0.2, // 80% active
                TypeOfShipment: (typeOfShipment)s_rand.Next(Enum.GetValues(typeof(typeOfShipment)).Length),
                StartingWork: DateTime.Now.AddDays(-s_rand.Next(0, 1500)), // up to 4 years back
                MaximumDistance: s_rand.NextDouble() > 0.3 ? s_rand.Next(5, 100) : null // some have unlimited range
            ));
        }
    }

    private static void CreateDelivery(int count = 15)
    {
        // Assuming you already created orders and couriers before
        var orders = s_dalOrder?.ReadAll() ?? [];
        var couriers = s_dalCourier?.ReadAll() ?? [];

        for (var i = 0; i < count; i++)
        {
            if (!orders.Any() || !couriers.Any()) break;

            var order = orders[s_rand.Next(orders.Count)];
            var courier = couriers[s_rand.Next(couriers.Count)];

            var startTime = DateTime.Now.AddHours(-s_rand.Next(0, 200));
            DateTime? endTime = s_rand.NextDouble() > 0.5 ? startTime.AddMinutes(s_rand.Next(10, 120)) : null;

            s_dalDelivery?.Create(new Delivery(
                Id: 0,
                OrderId: order.Id,
                CourierId: courier.Id,
                ShipmentType: (ShipmentType)s_rand.Next(Enum.GetValues(typeof(ShipmentType)).Length),
                StartTime: startTime,
                ActualDistance: s_rand.NextDouble() * 100,
                DeliveryEndType: endTime != null ? (DeliveryEndType)s_rand.Next(Enum.GetValues(typeof(DeliveryEndType)).Length) : null,
                EndTime: endTime
            ));
        }
    }
}
