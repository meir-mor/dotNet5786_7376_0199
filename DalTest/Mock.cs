using DO;

namespace DalTest;

public static class Mock
{
    public static readonly (string Address, ShipmentType TypeOfShipment)[] Addresses =
        [
            // Herzliya (mostly bicycle/motorcycle range)
            ("Wingate 38 Herzliya", ShipmentType.Bicycle),
            ("Abba Eban 2 Herzliya", ShipmentType.Motorcycle),
            ("Arlozorov 4 Herzliya", ShipmentType.Bicycle),
            ("Ben Yehuda 12 Herzliya", ShipmentType.Bicycle),
            ("Hagalil 11 Herzliya", ShipmentType.Bicycle),
            ("Maskit 27 Herzliya", ShipmentType.Motorcycle),
            ("Ben Gurion 12 Herzliya", ShipmentType.Bicycle),
            ("Ramat Yam 60 Herzliya", ShipmentType.Motorcycle),
            ("Weizmann 8 Kfar Saba", ShipmentType.Motorcycle),
            ("Shenkar 3 Herzliya", ShipmentType.Bicycle),
            
            // Netanya (mostly car deliveries)
            ("Herzl 20 Netanya", ShipmentType.Car),
            ("Ben Gurion 1 Netanya", ShipmentType.Car),
            ("Haatzmaut 34 Sderot", ShipmentType.Car),
            ("Tchernichovsky 7 Netanya", ShipmentType.Car),
            ("Dizengoff 16 Netanya", ShipmentType.Car),
            ("Sheshet Hayamim 3 Netanya", ShipmentType.Car),
            ("Ruppin 21 Netanya", ShipmentType.Car),
            ("HaNeviim 18 Netanya", ShipmentType.Car),
            ("Ben Ami 28 Netanya", ShipmentType.Car),
            ("Tchernichovsky 32 Netanya", ShipmentType.Car),
            
            // Ra'anana (motorcycle range)
            ("Ahuza 124 Raanana", ShipmentType.Motorcycle),
            ("HaPalmach 18 Raanana", ShipmentType.Motorcycle),
            ("Weizmann 40 Kfar Saba", ShipmentType.Motorcycle),
            ("Herzl 53 Raanana", ShipmentType.Motorcycle),
            ("Nordau 5 Raanana", ShipmentType.Motorcycle),
            ("HaRav Kook 10 Raanana", ShipmentType.Motorcycle),
            ("HaYovel 16 Raanana", ShipmentType.Motorcycle),
            ("Ben Gurion 5 Raanana", ShipmentType.Motorcycle),
            ("HaEshel 7 Raanana", ShipmentType.Motorcycle),
            ("Dizengoff 48 Tel Aviv", ShipmentType.Bicycle),
            
            // Tel Aviv (walk/bicycle range)
            ("Rothschild 22 Tel Aviv", ShipmentType.Walk),
            ("Ibn Gabirol 15 Tel Aviv", ShipmentType.Walk),
            ("Dizengoff 102 Tel Aviv", ShipmentType.Walk),
            ("Allenby 99 Tel Aviv", ShipmentType.Walk),
            ("King George 29 Tel Aviv", ShipmentType.Walk),
            ("Shenkin 17 Tel Aviv", ShipmentType.Walk),
            ("Bograshov 20 Tel Aviv", ShipmentType.Walk),
            ("Frishman 22 Tel Aviv", ShipmentType.Walk),
            ("Yehuda HaMaccabi 15 Tel Aviv", ShipmentType.Walk),
            ("Mikve Israel 19 Tel Aviv", ShipmentType.Bicycle),
            
            // Kfar Saba (motorcycle range)
            ("Weizmann 35 Kfar Saba", ShipmentType.Motorcycle),
            ("Herzl 12 Kfar Saba", ShipmentType.Motorcycle),
            ("Weizmann 50 Kfar Saba", ShipmentType.Motorcycle),
            ("Arlozorov 25 Kfar Saba", ShipmentType.Motorcycle),
            ("Dizengoff 55 Tel Aviv", ShipmentType.Walk),
            ("Herzl 78 Kfar Saba", ShipmentType.Motorcycle),
            ("Tel Hai 33 Kfar Saba", ShipmentType.Motorcycle),
            ("HaRav Maimon 6 Herzliya", ShipmentType.Bicycle),
            ("Rothschild 9 Kfar Saba", ShipmentType.Motorcycle),
            ("Sokolov 3 Kfar Saba", ShipmentType.Motorcycle)
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