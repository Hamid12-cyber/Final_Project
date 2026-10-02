namespace MotoHM.Api.Modules.Motorcycles;

// Bütün marka və modellər. Saytda elan olmasa belə filtr panelində (0) ilə görünür.
// Yeni marka/model əlavə etmək üçün bura yaz.
public static class MotorcycleCatalog
{
    public static readonly Dictionary<string, string[]> Brands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Honda"] = new[] { "CB125F", "CB190R", "CB500F", "CB650R", "CBR500R", "CBR650R", "CBR1000RR", "CRF250L", "Africa Twin", "NC750X", "Transalp", "Forza", "PCX", "SH", "Gold Wing", "Rebel 500", "Shadow" },
        ["Yamaha"] = new[] { "YBR125", "FZ-S", "MT-03", "MT-07", "MT-09", "MT-10", "R3", "R6", "R7", "R1", "Tenere 700", "Tracer 9", "XMAX", "NMAX", "Aerox", "Virago", "Drag Star" },
        ["Kawasaki"] = new[] { "Z400", "Z650", "Z900", "Z1000", "Ninja 400", "Ninja 650", "Ninja ZX-6R", "Ninja ZX-10R", "Versys 650", "Versys 1000", "Vulcan S", "KLX 230", "W800" },
        ["Suzuki"] = new[] { "GSX-S750", "GSX-S1000", "GSX-R600", "GSX-R750", "GSX-R1000", "SV650", "V-Strom 650", "V-Strom 1050", "Hayabusa", "Burgman 400", "Intruder", "Boulevard", "DR-Z400" },
        ["KTM"] = new[] { "Duke 125", "Duke 200", "Duke 390", "Duke 790", "Duke 890", "RC 390", "Adventure 390", "Adventure 790", "Adventure 1290", "EXC 300", "SX-F 250" },
        ["BMW"] = new[] { "G 310 R", "G 310 GS", "F 750 GS", "F 850 GS", "F 900 R", "R 1250 GS", "R 1250 R", "R nineT", "S 1000 RR", "S 1000 R", "C 400 GT" },
        ["Ducati"] = new[] { "Monster", "Scrambler", "Panigale V2", "Panigale V4", "Streetfighter V4", "Multistrada V2", "Multistrada V4", "Diavel", "Hypermotard", "DesertX" },
        ["Harley-Davidson"] = new[] { "Sportster S", "Iron 883", "Forty-Eight", "Street Bob", "Fat Boy", "Fat Bob", "Road King", "Street Glide", "Softail Slim", "Pan America" },
        ["Triumph"] = new[] { "Street Triple", "Speed Triple", "Trident 660", "Tiger 660", "Tiger 900", "Tiger 1200", "Bonneville T100", "Bonneville T120", "Scrambler 900", "Thruxton" },
        ["Aprilia"] = new[] { "RS 125", "RS 660", "RSV4", "Tuono 660", "Tuono V4", "Tuareg 660", "SR GT", "SX 125" },
        ["Royal Enfield"] = new[] { "Classic 350", "Bullet 350", "Meteor 350", "Hunter 350", "Himalayan", "Interceptor 650", "Continental GT 650", "Scram 411" },
        ["Vespa"] = new[] { "Primavera", "Sprint", "GTS 300", "GTV", "Elettrica" },
        ["Piaggio"] = new[] { "Liberty", "Medley", "Beverly", "MP3", "Zip" },
        ["Benelli"] = new[] { "TNT 125", "TNT 300", "TNT 600", "Leoncino 500", "TRK 502", "TRK 702", "502C", "Imperiale 400" },
        ["CFMoto"] = new[] { "250 NK", "300 SR", "450 SR", "650 NK", "650 MT", "700 CL-X", "800 MT" },
        ["Husqvarna"] = new[] { "Svartpilen 401", "Vitpilen 401", "Svartpilen 701", "Norden 901", "FE 350", "TC 250" },
        ["Voge"] = new[] { "300 R", "500 R", "500 DS", "525 R", "650 DS", "Valico 525" },
        ["Zontes"] = new[] { "125 U", "350 D", "350 T", "310 X", "350 R", "368 G" },
        ["SYM"] = new[] { "Symphony", "Jet 14", "Joyride", "Maxsym", "Cruisym" },
        ["Kymco"] = new[] { "Agility", "Like", "Downtown", "AK 550", "Xciting", "Super 8" },
        ["Bajaj"] = new[] { "Pulsar 150", "Pulsar NS200", "Pulsar 220F", "Dominar 400", "Avenger" },
        ["TVS"] = new[] { "Apache RTR 160", "Apache RTR 200", "Apache RR 310", "Ronin", "Jupiter" },
        ["Hero"] = new[] { "Splendor", "HF Deluxe", "Xtreme 160R", "Xpulse 200", "Karizma" },
        ["Lifan"] = new[] { "KP 200", "KPR 200", "KPT 200", "LF 150" },
        ["Racer"] = new[] { "Tiger 200", "Skyway 200", "Panther 200", "Ranger 250", "Storm 125" },
        ["Musstang"] = new[] { "MT 125", "MT 200", "Region 150", "Fox 150" },
        ["Dayun"] = new[] { "Digər" },
        ["Tufan"] = new[] { "Digər" },
        ["Kuba"] = new[] { "Digər" },
        ["Skymoto"] = new[] { "Digər" },
        ["Loncin"] = new[] { "Digər" },
        ["Zongshen"] = new[] { "Digər" },
        ["Shineray"] = new[] { "Digər" },
        ["Irbis"] = new[] { "Digər" },
        ["Haojue"] = new[] { "Digər" },
        ["Kayo"] = new[] { "Digər" },
        ["Bashan"] = new[] { "Digər" },
        ["Mikilon"] = new[] { "Digər" },
        ["Wels"] = new[] { "Digər" },
        ["Regal Raptor"] = new[] { "Digər" },
        ["Jawa"] = new[] { "Jawa 42", "Perak", "Digər" },
    };
}