using DO;

namespace DalTest;

public static class Mock
{
    public static readonly (string Address, double Latitude, double Longitude, double DistanceFromBarCochvaKm,
        ShipmentType TypeOfShipment)[] Addresses =
        [
            // Herzliya (mostly bicycle/motorcycle range)
            ("Wingate 38 Herzliya", 32.1645, 34.8268, 9.2, ShipmentType.Bicycle),
            ("Abba Eban 2 Herzliya", 32.1612, 34.8065, 10.5, ShipmentType.Motorcycle),
            ("Arlozorov 4 Herzliya", 32.1657, 34.8369, 8.8, ShipmentType.Bicycle),
            ("Ben Yehuda 12 Herzliya", 32.1624, 34.8351, 8.7, ShipmentType.Bicycle),
            ("Hagalil 11 Herzliya", 32.1701, 34.8243, 9.5, ShipmentType.Bicycle),
            ("Maskit 27 Herzliya", 32.1621, 34.8058, 10.3, ShipmentType.Motorcycle),
            ("Ben Gurion 12 Herzliya", 32.1642, 34.8317, 9.0, ShipmentType.Bicycle),
            ("Ramat Yam 60 Herzliya", 32.1752, 34.7936, 11.2, ShipmentType.Motorcycle),
            ("Weizmann 8 Kfar Saba", 32.1750, 34.9122, 18.4, ShipmentType.Motorcycle),
            ("Shenkar 3 Herzliya", 32.1656, 34.8107, 9.8, ShipmentType.Bicycle),

            // Netanya (mostly car deliveries)
            ("Herzl 20 Netanya", 32.3329, 34.8594, 31.2, ShipmentType.Car),
            ("Ben Gurion 1 Netanya", 32.3337, 34.8541, 31.0, ShipmentType.Car),
            ("Haatzmaut 34 Sderot", 32.3315, 34.8582, 31.1, ShipmentType.Car),
            ("Tchernichovsky 7 Netanya", 32.3265, 34.8547, 30.9, ShipmentType.Car),
            ("Dizengoff 16 Netanya", 32.3290, 34.8605, 31.3, ShipmentType.Car),
            ("Sheshet Hayamim 3 Netanya", 32.3198, 34.8712, 32.0, ShipmentType.Car),
            ("Ruppin 21 Netanya", 32.3421, 34.8536, 32.5, ShipmentType.Car),
            ("HaNeviim 18 Netanya", 32.3299, 34.8599, 31.2, ShipmentType.Car),
            ("Ben Ami 28 Netanya", 32.3323, 34.8492, 30.7, ShipmentType.Car),
            ("Tchernichovsky 32 Netanya", 32.3250, 34.8577, 31.0, ShipmentType.Car),

            // Ra'anana (motorcycle range)
            ("Ahuza 124 Raanana", 32.1823, 34.8731, 17.2, ShipmentType.Motorcycle),
            ("HaPalmach 18 Raanana", 32.1809, 34.8728, 17.0, ShipmentType.Motorcycle),
            ("Weizmann 40 Kfar Saba", 32.1789, 34.9128, 18.6, ShipmentType.Motorcycle),
            ("Herzl 53 Raanana", 32.1841, 34.8694, 17.1, ShipmentType.Motorcycle),
            ("Nordau 5 Raanana", 32.1825, 34.8772, 17.3, ShipmentType.Motorcycle),
            ("HaRav Kook 10 Raanana", 32.1794, 34.8778, 17.4, ShipmentType.Motorcycle),
            ("HaYovel 16 Raanana", 32.1849, 34.8661, 17.0, ShipmentType.Motorcycle),
            ("Ben Gurion 5 Raanana", 32.1776, 34.8733, 17.2, ShipmentType.Motorcycle),
            ("HaEshel 7 Raanana", 32.1831, 34.8705, 17.1, ShipmentType.Motorcycle),
            ("Dizengoff 48 Tel Aviv", 32.0790, 34.7740, 5.8, ShipmentType.Bicycle),

            // Tel Aviv (walk/bicycle range)
            ("Rothschild 22 Tel Aviv", 32.0629, 34.7722, 5.4, ShipmentType.Walk),
            ("Ibn Gabirol 15 Tel Aviv", 32.0800, 34.7815, 5.1, ShipmentType.Walk),
            ("Dizengoff 102 Tel Aviv", 32.0833, 34.7734, 5.2, ShipmentType.Walk),
            ("Allenby 99 Tel Aviv", 32.0687, 34.7701, 5.5, ShipmentType.Walk),
            ("King George 29 Tel Aviv", 32.0735, 34.7742, 5.3, ShipmentType.Walk),
            ("Shenkin 17 Tel Aviv", 32.0688, 34.7728, 5.4, ShipmentType.Walk),
            ("Bograshov 20 Tel Aviv", 32.0795, 34.7714, 5.1, ShipmentType.Walk),
            ("Frishman 22 Tel Aviv", 32.0806, 34.7725, 5.1, ShipmentType.Walk),
            ("Yehuda HaMaccabi 15 Tel Aviv", 32.0922, 34.7897, 4.8, ShipmentType.Walk),
            ("Mikve Israel 19 Tel Aviv", 32.0565, 34.7729, 5.6, ShipmentType.Bicycle),

            // Kfar Saba (motorcycle range)
            ("Weizmann 35 Kfar Saba", 32.1783, 34.9109, 18.3, ShipmentType.Motorcycle),
            ("Herzl 12 Kfar Saba", 32.1768, 34.9073, 18.2, ShipmentType.Motorcycle),
            ("Weizmann 50 Kfar Saba", 32.1792, 34.9127, 18.5, ShipmentType.Motorcycle),
            ("Arlozorov 25 Kfar Saba", 32.1777, 34.9088, 18.2, ShipmentType.Motorcycle),
            ("Dizengoff 55 Tel Aviv", 32.0815, 34.7743, 5.1, ShipmentType.Walk),
            ("Herzl 78 Kfar Saba", 32.1760, 34.9062, 18.1, ShipmentType.Motorcycle),
            ("Tel Hai 33 Kfar Saba", 32.1754, 34.9103, 18.3, ShipmentType.Motorcycle),
            ("HaRav Maimon 6 Herzliya", 32.1697, 34.8204, 9.6, ShipmentType.Bicycle),
            ("Rothschild 9 Kfar Saba", 32.1765, 34.9112, 18.4, ShipmentType.Motorcycle),
            ("Sokolov 3 Kfar Saba", 32.1779, 34.9095, 18.3, ShipmentType.Motorcycle)
        ];

    public static readonly string[] CustomerName =
    [
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
    ];

    public static readonly string[] CourierNames =
    [
        "Ori Zigi", "Itay Blum", "Amit Grossman", "Tal Shapiro", "Yossi Mizrachi",
        "Shira Ben-David", "Sarah Cohen", "Michal Avraham", "Nir Carmi", "Gal Noy",
        "Ronen Fisher", "Yael Katz", "Hila Raz", "Noam Peretz", "Maya Solomon",
        "Erez Ben-Yosef", "Avital Ashkenazi", "Ariel Ben-Ami", "Tzvika Mor", "Lior Shani"
    ];
}