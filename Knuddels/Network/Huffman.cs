using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Knuddels.Network
{
    /* Kommt nicht mal ansatzweiße an die Klasse von damals ran aber ich setz mich jetzt erstmal an den Shit wofür ich bezahlt werde
     * Issues:
     *     Compressed Length stimmt nicht
     *          Es wird nicht das richtige KeyValue Pair gefunden
     *       
     *       
     *     
     *     Geschwindigkeit muss noch optimiert werden (das Suchen in der _tree Dictionary sollte dafür verantworlich sein,
     *     aber so ist das halt wenn man mit Strings als Key arbeitet
     *     
     *     Also doch noch ne Möglichkeit suchen den Key anders zu bestimmen,
     *     gefällt mir aktuell eh noch nicht so ganz, aber für einzelne Verbindungen tut es das auf jeden Fall erstmal,
     *     für Server Anwendungen ( sollten aktuell aber doch lieber die "Original Files" genutzt werden,
     *     aber ich werde auch nicht aufhören bis es mir passt.
     *     
     *     Beispiel:
                Bytes: 54
                Das 1871 gegründete Deutsche Reich entwickelte sich rasch vom Agrar- zum Industriestaat. 
                Elapsed (C#): 20 ms.
                Bytes: 54
                Das 1871 gegründete Deutsche Reich entwickelte sich rasch vom Agrar- zum Industriestaat. 
                Elapsed (Applet): 3 ms.
     *     
     *     
     */

    /// <summary>
    /// Knuddels Huffman Komprimierung
    /// wird benötigt um Pakete an Knuddels zu senden und zu lesen.
    /// </summary>
    public class Huffman
    {
        /// <summary>
        /// Hier wird der berechnete Tree in einer Dictionary zur einfachen Verwendung gespeichert.
        /// Braucht mehr Ressourcen (im .NET Fiddle benchmark bei 1000 Kompressionen über 1 GB-Ram was ich lokal nicht reproduzieren kann) 
        /// aufgrund der suche in Dictionary (normal für Dictionarys im Vergleich zu Arrays) dafür ist der Code übersichtlicher, verständlicher und 
        /// macht bisher keine Probleme bei einer realen Verbindung zu Knuddels. 
        /// </summary>
        private readonly Dictionary<string, string> _tree;

        /// <summary>
        /// Gibt den Indikator für 16-Bit Zeichen an, wird im Konstruktor berechnet.
        /// wird für die Komprimierung gebraucht und vor den Zeichen 
        /// als Opcode in den BitStream geschrieben.
        /// </summary>
        private readonly string _16BitCharIndicator;
        
        /// <summary>
        /// Wird als Hilfs-Buffer verwendet:
        ///     Konstrktor: Binary Werte berechnen
        ///     Compress: Zeichen, Zecuhenketten im Tree finden
        ///     Decompress: Bit-Werte berechnen
        /// </summary>
        private readonly StringBuilder _helper;

        #region Konstruktor | Create Tree

        /// <summary>
        /// Erstellt eine neue Instanz der Huffman-Komprimierung
        /// mit Hilfe des Knuddels-Trees (Stand: k90cab . 12.05.2020)
        /// </summary>
        public Huffman()
            : this(new string(new char[] { (char)8, (char)121, (char)239, (char)124, (char)47, (char)115, (char)101, (char)114, (char)118, (char)101, (char)114, (char)112, (char)112, (char)32, (char)34, (char)30, (char)116, (char)116, (char)9, (char)68, (char)33, (char)101, (char)97, (char)34, (char)117, (char)100, (char)15, (char)131, (char)17, (char)1079, (char)18, (char)218, (char)18, (char)222, (char)16, (char)8364, (char)15, (char)8254, (char)15, (char)133, (char)33, (char)252, (char)114, (char)12, (char)200, (char)53, (char)83, (char)116, (char)101, (char)53, (char)101, (char)99, (char)107, (char)31, (char)48, (char)54, (char)95, (char)77, (char)97, (char)102, (char)105, (char)97, (char)34, (char)54, (char)55, (char)34, (char)78, (char)73, (char)34, (char)108, (char)98, (char)34, (char)111, (char)117, (char)6, (char)35, (char)5, (char)10, (char)4, (char)176, (char)31, (char)115, (char)105, (char)52, (char)112, (char)108, (char)97, (char)31, (char)116, (char)105, (char)52, (char)102, (char)111, (char)114, (char)8, (char)12, (char)53, (char)98, (char)111, (char)121, (char)11, (char)13, (char)53, (char)109, (char)97, (char)110, (char)33, (char)75, (char)108, (char)34, (char)57, (char)55, (char)34, (char)117, (char)98, (char)53, (char)117, (char)114, (char)103, (char)53, (char)107, (char)116, (char)101, (char)32, (char)79, (char)75, (char)32, (char)117, (char)101, (char)8, (char)98, (char)32, (char)111, (char)112, (char)33, (char)101, (char)118, (char)33, (char)79, (char)98, (char)54, (char)112, (char)101, (char)114, (char)33, (char)75, (char)111, (char)32, (char)77, (char)111, (char)93, (char)95, (char)176, (char)62, (char)95, (char)104, (char)8, (char)109, (char)31, (char)100, (char)105, (char)31, (char)114, (char)97, (char)11, (char)106, (char)15, (char)130, (char)18, (char)970, (char)18, (char)217, (char)17, (char)198, (char)16, (char)353, (char)15, (char)185, (char)16, (char)9644, (char)17, (char)201, (char)17, (char)248, (char)34, (char)53, (char)51, (char)34, (char)108, (char)104, (char)14, (char)8226, (char)14, (char)96, (char)31, (char)112, (char)121, (char)30, (char)49, (char)51, (char)30, (char)110, (char)110, (char)7, (char)227, (char)32, (char)114, (char)99, (char)200, (char)95, (char)110, (char)105, (char)99, (char)107, (char)108, (char)105, (char)115, (char)116, (char)95, (char)32, (char)101, (char)102, (char)53, (char)98, (char)114, (char)97, (char)31, (char)101, (char)101, (char)53, (char)116, (char)101, (char)110, (char)32, (char)110, (char)99, (char)52, (char)63, (char)100, (char)61, (char)32, (char)87, (char)111, (char)34, (char)55, (char)55, (char)13, (char)250, (char)33, (char)57, (char)52, (char)52, (char)105, (char)110, (char)103, (char)11, (char)14, (char)32, (char)48, (char)50, (char)31, (char)68, (char)101, (char)31, (char)97, (char)117, (char)33, (char)82, (char)117, (char)34, (char)111, (char)121, (char)34, (char)107, (char)109, (char)53, (char)101, (char)105, (char)103, (char)32, (char)69, (char)115, (char)33, (char)107, (char)115, (char)33, (char)79, (char)108, (char)8, (char)53, (char)9, (char)69, (char)31, (char)105, (char)105, (char)11, (char)22, (char)34, (char)109, (char)102, (char)34, (char)70, (char)111, (char)34, (char)115, (char)107, (char)16, (char)178, (char)17, (char)9565, (char)17, (char)158, (char)18, (char)64380, (char)18, (char)65185, (char)18, (char)65190, (char)18, (char)8217, (char)17, (char)281, (char)18, (char)64398, (char)18, (char)65270, (char)14, (char)21, (char)32, (char)69, (char)114, (char)54, (char)117, (char)115, (char)101, (char)33, (char)79, (char)110, (char)32, (char)111, (char)108, (char)33, (char)56, (char)57, (char)34, (char)107, (char)246, (char)34, (char)90, (char)101, (char)52, (char)97, (char)114, (char)116, (char)31, (char)114, (char)105, (char)8, (char)119, (char)32, (char)99, (char)111, (char)14, (char)25, (char)14, (char)196, (char)34, (char)67, (char)116, (char)33, (char)118, (char)97, (char)31, (char)98, (char)105, (char)52, (char)108, (char)105, (char)110, (char)32, (char)104, (char)114, (char)32, (char)105, (char)100, (char)32, (char)100, (char)111, (char)32, (char)110, (char)107, (char)32, (char)120, (char)120, (char)34, (char)119, (char)98, (char)14, (char)24, (char)16, (char)1082, (char)16, (char)205, (char)15, (char)139, (char)34, (char)112, (char)104, (char)34, (char)102, (char)119, (char)53, (char)110, (char)111, (char)99, (char)32, (char)119, (char)101, (char)52, (char)121, (char)101, (char)114, (char)31, (char)102, (char)101, (char)32, (char)107, (char)111, (char)32, (char)104, (char)116, (char)31, (char)109, (char)105, (char)53, (char)49, (char)48, (char)48, (char)32, (char)51, (char)49, (char)7, (char)51, (char)7, (char)108, (char)10, (char)87, (char)31, (char)102, (char)99, (char)34, (char)107, (char)252, (char)15, (char)1074, (char)16, (char)249, (char)18, (char)1171, (char)18, (char)8482, (char)17, (char)1072, (char)14, (char)180, (char)33, (char)105, (char)122, (char)32, (char)48, (char)57, (char)31, (char)66, (char)78, (char)30, (char)105, (char)109, (char)9, (char)1, (char)50, (char)0, (char)112, (char)0, (char)33, (char)75, (char)114, (char)34, (char)79, (char)111, (char)34, (char)101, (char)103, (char)53, (char)80, (char)117, (char)110, (char)32, (char)121, (char)108, (char)32, (char)101, (char)104, (char)32, (char)82, (char)111, (char)32, (char)83, (char)117, (char)33, (char)120, (char)101, (char)54, (char)104, (char)101, (char)108, (char)53, (char)104, (char)108, (char)116, (char)31, (char)101, (char)116, (char)31, (char)115, (char)109, (char)10, (char)77, (char)32, (char)112, (char)97, (char)32, (char)51, (char)56, (char)9, (char)122, (char)33, (char)70, (char)108, (char)33, (char)52, (char)56, (char)32, (char)117, (char)102, (char)53, (char)75, (char)110, (char)117, (char)32, (char)78, (char)105, (char)32, (char)104, (char)105, (char)34, (char)108, (char)122, (char)13, (char)220, (char)54, (char)104, (char)101, (char)114, (char)32, (char)83, (char)111, (char)34, (char)83, (char)252, (char)34, (char)117, (char)104, (char)15, (char)8594, (char)15, (char)9617, (char)14, (char)1085, (char)34, (char)114, (char)252, (char)30, (char)70, (char)104, (char)52, (char)77, (char)105, (char)110, (char)31, (char)98, (char)97, (char)31, (char)112, (char)108, (char)94, (char)48, (char)44, (char)48, (char)44, (char)48, (char)30, (char)49, (char)55, (char)30, (char)32, (char)32, (char)53, (char)98, (char)105, (char)110, (char)33, (char)71, (char)108, (char)33, (char)78, (char)117, (char)115, (char)77, (char)97, (char)102, (char)105, (char)97, (char)50, (char)31, (char)100, (char)97, (char)33, (char)82, (char)104, (char)54, (char)107, (char)101, (char)110, (char)32, (char)83, (char)108, (char)10, (char)73, (char)137, (char)107, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)32, (char)70, (char)98, (char)31, (char)66, (char)111, (char)33, (char)84, (char)111, (char)34, (char)122, (char)97, (char)16, (char)402, (char)17, (char)181, (char)17, (char)9619, (char)15, (char)134, (char)14, (char)1080, (char)33, (char)71, (char)111, (char)33, (char)53, (char)54, (char)53, (char)111, (char)105, (char)115, (char)32, (char)99, (char)101, (char)10, (char)78, (char)32, (char)48, (char)51, (char)33, (char)114, (char)98, (char)33, (char)104, (char)110, (char)53, (char)114, (char)105, (char)101, (char)33, (char)74, (char)101, (char)33, (char)68, (char)114, (char)8, (char)52, (char)53, (char)70, (char)117, (char)223, (char)33, (char)74, (char)117, (char)34, (char)110, (char)121, (char)34, (char)89, (char)111, (char)31, (char)51, (char)54, (char)53, (char)104, (char)101, (char)105, (char)34, (char)53, (char)56, (char)34, (char)111, (char)118, (char)12, (char)36, (char)31, (char)66, (char)66, (char)8, (char)38, (char)94, (char)66, (char)105, (char)110, (char)103, (char)111, (char)53, (char)108, (char)105, (char)101, (char)33, (char)98, (char)121, (char)33, (char)73, (char)103, (char)52, (char)115, (char)116, (char)101, (char)10, (char)17, (char)7, (char)50, (char)10, (char)71, (char)32, (char)105, (char)99, (char)54, (char)107, (char)108, (char)101, (char)18, (char)65275, (char)18, (char)1071, (char)17, (char)1106, (char)16, (char)224, (char)16, (char)1751, (char)18, (char)1077, (char)18, (char)193, (char)18, (char)213, (char)18, (char)9474, (char)17, (char)238, (char)17, (char)9556, (char)16, (char)127, (char)15, (char)214, (char)34, (char)54, (char)52, (char)31, (char)115, (char)101, (char)31, (char)50, (char)51, (char)31, (char)110, (char)103, (char)32, (char)119, (char)97, (char)33, (char)68, (char)252, (char)33, (char)102, (char)108, (char)31, (char)97, (char)114, (char)32, (char)114, (char)115, (char)32, (char)66, (char)105, (char)132, (char)0, (char)112, (char)0, (char)66, (char)0, (char)45, (char)0, (char)7, (char)245, (char)7, (char)44, (char)8, (char)194, (char)52, (char)117, (char)116, (char)101, (char)31, (char)118, (char)101, (char)10, (char)64, (char)33, (char)115, (char)108, (char)33, (char)97, (char)99, (char)32, (char)72, (char)101, (char)7, (char)66, (char)29, (char)50, (char)48, (char)32, (char)87, (char)105, (char)34, (char)99, (char)108, (char)34, (char)77, (char)246, (char)33, (char)100, (char)109, (char)31, (char)77, (char)97, (char)30, (char)108, (char)101, (char)7, (char)105, (char)32, (char)111, (char)116, (char)34, (char)115, (char)99, (char)34, (char)117, (char)117, (char)12, (char)9, (char)10, (char)83, (char)53, (char)103, (char)101, (char)98, (char)32, (char)77, (char)101, (char)255, (char)16, (char)11, (char)102, (char)101, (char)109, (char)97, (char)108, (char)101, (char)46, (char)98, (char)46, (char)109, (char)121, (char)95, (char)52, (char)46, (char)103, (char)105, (char)102, (char)32, (char)116, (char)108, (char)30, (char)49, (char)54, (char)52, (char)99, (char)104, (char)116, (char)34, (char)108, (char)228, (char)34, (char)112, (char)117, (char)34, (char)115, (char)98, (char)34, (char)53, (char)49, (char)53, (char)70, (char)105, (char)102, (char)32, (char)57, (char)48, (char)53, (char)101, (char)114, (char)104, (char)52, (char)50, (char)53, (char)53, (char)10, (char)75, (char)31, (char)105, (char)103, (char)8, (char)124, (char)30, (char)109, (char)97, (char)32, (char)50, (char)57, (char)34, (char)65, (char)109, (char)15, (char)9835, (char)16, (char)8250, (char)17, (char)157, (char)17, (char)160, (char)17, (char)254, (char)17, (char)8592, (char)17, (char)153, (char)17, (char)321, (char)15, (char)1084, (char)33, (char)75, (char)246, (char)53, (char)118, (char)111, (char)110, (char)32, (char)75, (char)105, (char)34, (char)228, (char)117, (char)34, (char)67, (char)101, (char)34, (char)55, (char)56, (char)34, (char)71, (char)105, (char)32, (char)110, (char)117, (char)52, (char)103, (char)101, (char)110, (char)32, (char)100, (char)117, (char)54, (char)77, (char)97, (char)114, (char)33, (char)114, (char)117, (char)33, (char)118, (char)111, (char)33, (char)106, (char)117, (char)13, (char)8467, (char)34, (char)98, (char)115, (char)54, (char)117, (char)104, (char)112, (char)219, (char)70, (char)111, (char)116, (char)111, (char)67, (char)111, (char)110, (char)116, (char)101, (char)115, (char)116, (char)32, (char)67, (char)112, (char)54, (char)116, (char)111, (char)114, (char)54, (char)114, (char)101, (char)97, (char)32, (char)122, (char)105, (char)74, (char)107, (char)97, (char)110, (char)110, (char)30, (char)108, (char)108, (char)9, (char)63, (char)8, (char)251, (char)8, (char)190, (char)32, (char)109, (char)112, (char)33, (char)108, (char)110, (char)54, (char)83, (char)112, (char)105, (char)31, (char)117, (char)115, (char)30, (char)115, (char)116, (char)33, (char)114, (char)102, (char)13, (char)6, (char)13, (char)143, (char)32, (char)102, (char)102, (char)32, (char)115, (char)104, (char)33, (char)52, (char)52, (char)33, (char)73, (char)99, (char)52, (char)104, (char)97, (char)116, (char)32, (char)106, (char)97, (char)54, (char)46, (char)119, (char)95, (char)12, (char)20, (char)8, (char)103, (char)32, (char)50, (char)55, (char)34, (char)65, (char)114, (char)13, (char)1108, (char)33, (char)252, (char)98, (char)31, (char)105, (char)101, (char)30, (char)49, (char)48, (char)54, (char)85, (char)110, (char)100, (char)54, (char)105, (char)101, (char)115, (char)33, (char)57, (char)49, (char)14, (char)182, (char)15, (char)233, (char)16, (char)216, (char)18, (char)331, (char)18, (char)9600, (char)17, (char)231, (char)34, (char)73, (char)73, (char)32, (char)103, (char)111, (char)33, (char)111, (char)98, (char)12, (char)39, (char)219, (char)107, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)115, (char)46, (char)100, (char)101, (char)8, (char)120, (char)10, (char)91, (char)53, (char)110, (char)103, (char)101, (char)33, (char)108, (char)116, (char)34, (char)56, (char)56, (char)34, (char)90, (char)105, (char)32, (char)78, (char)97, (char)32, (char)70, (char)97, (char)10, (char)93, (char)8, (char)188, (char)10, (char)228, (char)53, (char)97, (char)117, (char)115, (char)74, (char)115, (char)105, (char)99, (char)104, (char)72, (char)105, (char)99, (char)111, (char)110, (char)9, (char)54, (char)12, (char)9608, (char)33, (char)71, (char)114, (char)32, (char)112, (char)111, (char)31, (char)51, (char)48, (char)7, (char)62, (char)52, (char)83, (char)99, (char)104, (char)54, (char)112, (char)97, (char)114, (char)33, (char)101, (char)121, (char)33, (char)71, (char)97, (char)54, (char)78, (char)111, (char)114, (char)93, (char)109, (char)97, (char)102, (char)105, (char)97, (char)30, (char)49, (char)50, (char)33, (char)74, (char)97, (char)33, (char)115, (char)252, (char)15, (char)184, (char)15, (char)138, (char)15, (char)27, (char)15, (char)135, (char)34, (char)246, (char)110, (char)54, (char)109, (char)97, (char)103, (char)52, (char)118, (char)101, (char)114, (char)32, (char)71, (char)101, (char)11, (char)164, (char)31, (char)50, (char)50, (char)33, (char)97, (char)103, (char)34, (char)55, (char)50, (char)14, (char)144, (char)15, (char)244, (char)15, (char)136, (char)53, (char)104, (char)97, (char)108, (char)31, (char)50, (char)49, (char)32, (char)114, (char)111, (char)33, (char)80, (char)115, (char)34, (char)53, (char)57, (char)34, (char)70, (char)117, (char)31, (char)108, (char)97, (char)32, (char)54, (char)48, (char)11, (char)80, (char)53, (char)83, (char)116, (char)117, (char)33, (char)109, (char)117, (char)34, (char)65, (char)97, (char)34, (char)54, (char)50, (char)7, (char)110, (char)7, (char)58, (char)32, (char)100, (char)116, (char)54, (char)79, (char)78, (char)76, (char)33, (char)57, (char)50, (char)94, (char)124, (char)47, (char)103, (char)111, (char)32, (char)136, (char)67, (char)104, (char)97, (char)110, (char)110, (char)101, (char)108, (char)53, (char)98, (char)101, (char)105, (char)32, (char)72, (char)105, (char)9, (char)2, (char)9, (char)56, (char)9, (char)102, (char)52, (char)50, (char)48, (char)48, (char)34, (char)102, (char)111, (char)34, (char)104, (char)109, (char)54, (char)114, (char)111, (char)115, (char)32, (char)103, (char)108, (char)8, (char)48, (char)33, (char)52, (char)57, (char)54, (char)101, (char)110, (char)115, (char)32, (char)51, (char)52, (char)54, (char)115, (char)119, (char)104, (char)33, (char)98, (char)116, (char)34, (char)72, (char)80, (char)18, (char)211, (char)18, (char)9618, (char)18, (char)199, (char)18, (char)209, (char)17, (char)8730, (char)17, (char)8595, (char)15, (char)226, (char)16, (char)177, (char)16, (char)926, (char)15, (char)132, (char)54, (char)66, (char)97, (char)100, (char)31, (char)101, (char)100, (char)12, (char)15, (char)54, (char)116, (char)100, (char)101, (char)74, (char)46, (char)112, (char)110, (char)103, (char)9, (char)100, (char)33, (char)109, (char)98, (char)33, (char)107, (char)110, (char)32, (char)68, (char)97, (char)32, (char)105, (char)114, (char)54, (char)71, (char)105, (char)114, (char)75, (char)50, (char)48, (char)48, (char)57, (char)29, (char)48, (char)48, (char)53, (char)80, (char)114, (char)105, (char)54, (char)66, (char)114, (char)101, (char)33, (char)84, (char)101, (char)34, (char)86, (char)101, (char)34, (char)53, (char)52, (char)33, (char)83, (char)104, (char)53, (char)87, (char)104, (char)111, (char)33, (char)102, (char)116, (char)33, (char)78, (char)101, (char)32, (char)88, (char)120, (char)94, (char)74, (char)97, (char)109, (char)101, (char)115, (char)32, (char)67, (char)108, (char)32, (char)109, (char)111, (char)54, (char)97, (char)110, (char)103, (char)12, (char)204, (char)33, (char)87, (char)117, (char)33, (char)48, (char)52, (char)9, (char)57, (char)74, (char)48, (char)48, (char)54, (char)53, (char)34, (char)67, (char)72, (char)34, (char)99, (char)105, (char)33, (char)103, (char)104, (char)31, (char)107, (char)101, (char)33, (char)117, (char)114, (char)33, (char)97, (char)107, (char)33, (char)105, (char)97, (char)33, (char)119, (char)117, (char)94, (char)112, (char)105, (char)99, (char)115, (char)47, (char)52, (char)100, (char)101, (char)110, (char)32, (char)76, (char)111, (char)53, (char)115, (char)116, (char)97, (char)34, (char)71, (char)66, (char)55, (char)115, (char)98, (char)117, (char)34, (char)111, (char)104, (char)34, (char)113, (char)117, (char)32, (char)83, (char)97, (char)31, (char)108, (char)105, (char)9, (char)97, (char)10, (char)79, (char)33, (char)70, (char)101, (char)34, (char)120, (char)121, (char)18, (char)163, (char)18, (char)1086, (char)17, (char)154, (char)16, (char)169, (char)17, (char)247, (char)17, (char)9559, (char)18, (char)9472, (char)18, (char)8220, (char)18, (char)173, (char)18, (char)189, (char)15, (char)171, (char)17, (char)9562, (char)18, (char)203, (char)18, (char)242, (char)17, (char)165, (char)17, (char)243, (char)74, (char)110, (char)105, (char)99, (char)104, (char)54, (char)98, (char)101, (char)115, (char)33, (char)116, (char)99, (char)12, (char)172, (char)12, (char)9829, (char)53, (char)109, (char)101, (char)110, (char)32, (char)83, (char)116, (char)52, (char)98, (char)117, (char)114, (char)31, (char)114, (char)116, (char)33, (char)57, (char)51, (char)33, (char)87, (char)97, (char)33, (char)84, (char)105, (char)33, (char)48, (char)55, (char)32, (char)103, (char)97, (char)33, (char)100, (char)115, (char)34, (char)57, (char)57, (char)34, (char)77, (char)117, (char)51, (char)115, (char)99, (char)104, (char)33, (char)74, (char)111, (char)54, (char)72, (char)101, (char)114, (char)34, (char)103, (char)103, (char)34, (char)118, (char)98, (char)33, (char)105, (char)98, (char)53, (char)101, (char)110, (char)116, (char)34, (char)56, (char)55, (char)34, (char)121, (char)97, (char)33, (char)108, (char)102, (char)32, (char)111, (char)114, (char)33, (char)101, (char)98, (char)33, (char)97, (char)119, (char)11, (char)18, (char)32, (char)116, (char)122, (char)29, (char)99, (char)104, (char)33, (char)114, (char)108, (char)159, (char)46, (char)115, (char)104, (char)97, (char)100, (char)111, (char)119, (char)95, (char)32, (char)115, (char)112, (char)33, (char)116, (char)111, (char)33, (char)111, (char)111, (char)11, (char)42, (char)30, (char)49, (char)53, (char)9, (char)111, (char)34, (char)110, (char)102, (char)34, (char)56, (char)54, (char)33, (char)57, (char)53, (char)54, (char)73, (char)78, (char)69, (char)33, (char)83, (char)105, (char)74, (char)70, (char)111, (char)116, (char)111, (char)33, (char)121, (char)115, (char)34, (char)57, (char)54, (char)15, (char)26, (char)15, (char)31, (char)15, (char)152, (char)16, (char)225, (char)17, (char)212, (char)18, (char)1111, (char)18, (char)236, (char)7, (char)114, (char)52, (char)105, (char)115, (char)116, (char)54, (char)76, (char)97, (char)100, (char)54, (char)77, (char)111, (char)109, (char)33, (char)84, (char)104, (char)54, (char)80, (char)114, (char)111, (char)32, (char)107, (char)97, (char)32, (char)67, (char)104, (char)31, (char)110, (char)105, (char)30, (char)115, (char)115, (char)73, (char)46, (char)109, (char)121, (char)95, (char)32, (char)118, (char)105, (char)54, (char)77, (char)111, (char)110, (char)33, (char)112, (char)105, (char)33, (char)81, (char)102, (char)34, (char)111, (char)99, (char)34, (char)115, (char)103, (char)34, (char)84, (char)97, (char)34, (char)116, (char)119, (char)34, (char)105, (char)102, (char)34, (char)122, (char)122, (char)31, (char)110, (char)97, (char)33, (char)65, (char)117, (char)33, (char)97, (char)105, (char)158, (char)75, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)115, (char)10, (char)88, (char)71, (char)176, (char)62, (char)95, (char)104, (char)35, (char)117, (char)103, (char)35, (char)77, (char)121, (char)34, (char)80, (char)101, (char)34, (char)101, (char)120, (char)34, (char)78, (char)111, (char)32, (char)75, (char)97, (char)32, (char)110, (char)111, (char)34, (char)120, (char)116, (char)34, (char)104, (char)112, (char)34, (char)107, (char)114, (char)34, (char)80, (char)117, (char)30, (char)49, (char)56, (char)9, (char)82, (char)53, (char)83, (char)105, (char)110, (char)32, (char)65, (char)110, (char)34, (char)82, (char)105, (char)55, (char)115, (char)101, (char)108, (char)54, (char)98, (char)101, (char)97, (char)33, (char)86, (char)111, (char)34, (char)97, (char)104, (char)34, (char)81, (char)117, (char)7, (char)43, (char)10, (char)99, (char)33, (char)116, (char)115, (char)33, (char)84, (char)114, (char)32, (char)87, (char)101, (char)10, (char)76, (char)241, (char)105, (char)99, (char)111, (char)110, (char)95, (char)103, (char)101, (char)110, (char)100, (char)101, (char)114, (char)95, (char)31, (char)109, (char)101, (char)53, (char)46, (char)104, (char)95, (char)54, (char)119, (char)119, (char)119, (char)34, (char)121, (char)111, (char)34, (char)104, (char)117, (char)255, (char)12, (char)10, (char)124, (char)47, (char)102, (char)111, (char)116, (char)111, (char)119, (char)104, (char)111, (char)105, (char)115, (char)32, (char)34, (char)53, (char)109, (char)105, (char)116, (char)32, (char)115, (char)111, (char)8, (char)11, (char)52, (char)70, (char)114, (char)101, (char)54, (char)109, (char)97, (char)120, (char)54, (char)114, (char)101, (char)103, (char)33, (char)86, (char)97, (char)54, (char)119, (char)101, (char)114, (char)10, (char)117, (char)54, (char)119, (char)105, (char)101, (char)117, (char)105, (char)99, (char)111, (char)110, (char)115, (char)47, (char)33, (char)69, (char)110, (char)15, (char)9552, (char)16, (char)149, (char)16, (char)215, (char)14, (char)159, (char)34, (char)106, (char)111, (char)7, (char)37, (char)220, (char)50, (char)53, (char)53, (char)44, (char)50, (char)53, (char)53, (char)44, (char)50, (char)53, (char)53, (char)32, (char)101, (char)105, (char)33, (char)100, (char)114, (char)33, (char)105, (char)112, (char)32, (char)68, (char)117, (char)34, (char)107, (char)102, (char)34, (char)53, (char)55, (char)33, (char)80, (char)108, (char)31, (char)108, (char)111, (char)53, (char)83, (char)116, (char)97, (char)14, (char)945, (char)15, (char)29, (char)17, (char)964, (char)17, (char)210, (char)16, (char)145, (char)34, (char)69, (char)82, (char)33, (char)107, (char)117, (char)32, (char)108, (char)115, (char)54, (char)97, (char)99, (char)116, (char)33, (char)70, (char)114, (char)10, (char)92, (char)255, (char)19, (char)11, (char)102, (char)111, (char)116, (char)111, (char)115, (char)47, (char)107, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)115, (char)46, (char)100, (char)101, (char)63, (char)110, (char)61, (char)11, (char)72, (char)50, (char)32, (char)32, (char)32, (char)33, (char)76, (char)117, (char)54, (char)100, (char)101, (char)109, (char)53, (char)101, (char)110, (char)100, (char)35, (char)66, (char)252, (char)35, (char)97, (char)112, (char)34, (char)55, (char)52, (char)54, (char)99, (char)104, (char)108, (char)53, (char)103, (char)108, (char)101, (char)30, (char)52, (char)48, (char)9, (char)33, (char)30, (char)51, (char)50, (char)52, (char)101, (char)105, (char)110, (char)53, (char)115, (char)101, (char)105, (char)33, (char)75, (char)101, (char)12, (char)223, (char)32, (char)102, (char)103, (char)53, (char)98, (char)97, (char)108, (char)31, (char)116, (char)97, (char)30, (char)49, (char)57, (char)52, (char)110, (char)100, (char)101, (char)54, (char)77, (char)101, (char)110, (char)54, (char)76, (char)105, (char)101, (char)32, (char)97, (char)100, (char)31, (char)83, (char)102, (char)10, (char)167, (char)30, (char)105, (char)110, (char)35, (char)101, (char)99, (char)15, (char)30, (char)16, (char)8249, (char)17, (char)221, (char)18, (char)166, (char)18, (char)234, (char)14, (char)953, (char)35, (char)72, (char)252, (char)33, (char)111, (char)115, (char)32, (char)114, (char)110, (char)74, (char)46, (char)109, (char)120, (char)95, (char)53, (char)119, (char)101, (char)105, (char)30, (char)46, (char)46, (char)54, (char)108, (char)101, (char)114, (char)33, (char)114, (char)107, (char)11, (char)113, (char)34, (char)116, (char)102, (char)34, (char)85, (char)110, (char)33, (char)80, (char)111, (char)53, (char)86, (char)101, (char)114, (char)9, (char)112, (char)7, (char)47, (char)12, (char)125, (char)35, (char)100, (char)119, (char)15, (char)1109, (char)16, (char)146, (char)16, (char)235, (char)34, (char)97, (char)122, (char)33, (char)108, (char)117, (char)33, (char)70, (char)105, (char)33, (char)107, (char)108, (char)33, (char)119, (char)111, (char)32, (char)56, (char)48, (char)255, (char)32, (char)9, (char)0, (char)112, (char)0, (char)66, (char)0, (char)112, (char)105, (char)99, (char)115, (char)47, (char)105, (char)99, (char)111, (char)110, (char)95, (char)102, (char)117, (char)108, (char)108, (char)67, (char)104, (char)97, (char)110, (char)110, (char)101, (char)108, (char)46, (char)103, (char)105, (char)102, (char)0, (char)45, (char)0, (char)30, (char)101, (char)108, (char)31, (char)116, (char)121, (char)54, (char)78, (char)101, (char)117, (char)34, (char)53, (char)50, (char)34, (char)108, (char)109, (char)32, (char)48, (char)49, (char)113, (char)124, (char)47, (char)119, (char)32, (char)34, (char)60, (char)180, (char)46, (char)113, (char)117, (char)97, (char)100, (char)99, (char)117, (char)116, (char)95, (char)35, (char)54, (char)57, (char)35, (char)100, (char)228, (char)34, (char)116, (char)117, (char)54, (char)102, (char)114, (char)101, (char)34, (char)105, (char)104, (char)35, (char)101, (char)122, (char)35, (char)57, (char)56, (char)31, (char)116, (char)101, (char)53, (char)103, (char)105, (char)114, (char)53, (char)99, (char)110, (char)116, (char)158, (char)46, (char)98, (char)111, (char)114, (char)100, (char)101, (char)114, (char)95, (char)32, (char)105, (char)116, (char)11, (char)89, (char)255, (char)14, (char)11, (char)102, (char)117, (char)108, (char)108, (char)67, (char)104, (char)97, (char)110, (char)110, (char)101, (char)108, (char)46, (char)103, (char)105, (char)102, (char)34, (char)122, (char)111, (char)34, (char)107, (char)99, (char)34, (char)65, (char)98, (char)15, (char)965, (char)18, (char)9642, (char)19, (char)9577, (char)19, (char)8494, (char)17, (char)202, (char)16, (char)141, (char)35, (char)56, (char)50, (char)32, (char)51, (char)51, (char)31, (char)110, (char)100, (char)10, (char)70, (char)32, (char)83, (char)112, (char)11, (char)94, (char)54, (char)97, (char)99, (char)104, (char)33, (char)117, (char)99, (char)32, (char)107, (char)105, (char)52, (char)105, (char)99, (char)104, (char)55, (char)102, (char)101, (char)108, (char)35, (char)56, (char)51, (char)15, (char)191, (char)15, (char)219, (char)33, (char)98, (char)114, (char)54, (char)70, (char)114, (char)105, (char)54, (char)100, (char)111, (char)114, (char)31, (char)100, (char)101, (char)31, (char)53, (char)48, (char)33, (char)97, (char)97, (char)34, (char)103, (char)98, (char)34, (char)108, (char)99, (char)54, (char)102, (char)252, (char)114, (char)33, (char)55, (char)53, (char)32, (char)116, (char)104, (char)53, (char)66, (char)101, (char)114, (char)54, (char)66, (char)97, (char)108, (char)33, (char)55, (char)48, (char)33, (char)52, (char)53, (char)34, (char)83, (char)119, (char)34, (char)52, (char)49, (char)32, (char)105, (char)108, (char)54, (char)104, (char)102, (char)108, (char)34, (char)73, (char)104, (char)13, (char)126, (char)255, (char)14, (char)10, (char)112, (char)105, (char)99, (char)115, (char)47, (char)102, (char)101, (char)109, (char)97, (char)108, (char)101, (char)46, (char)103, (char)105, (char)102, (char)53, (char)104, (char)116, (char)116, (char)16, (char)9834, (char)18, (char)161, (char)18, (char)232, (char)18, (char)305, (char)19, (char)8212, (char)62, (char)92, (char)92, (char)92, (char)20, (char)1241, (char)15, (char)183, (char)35, (char)65, (char)116, (char)35, (char)76, (char)69, (char)35, (char)83, (char)107, (char)75, (char)50, (char)48, (char)49, (char)48, (char)71, (char)46, (char)103, (char)105, (char)102, (char)74, (char)102, (char)111, (char)116, (char)111, (char)54, (char)76, (char)97, (char)110, (char)13, (char)19, (char)34, (char)105, (char)107, (char)199, (char)176, (char)62, (char)103, (char)116, (char)46, (char)103, (char)105, (char)102, (char)60, (char)176, (char)32, (char)72, (char)111, (char)34, (char)100, (char)121, (char)34, (char)80, (char)105, (char)54, (char)97, (char)108, (char)116, (char)32, (char)50, (char)56, (char)74, (char)46, (char)106, (char)112, (char)103, (char)7, (char)101, (char)8, (char)115, (char)32, (char)69, (char)108, (char)54, (char)109, (char)101, (char)114, (char)255, (char)14, (char)12, (char)109, (char)97, (char)108, (char)101, (char)46, (char)98, (char)46, (char)109, (char)121, (char)95, (char)51, (char)46, (char)103, (char)105, (char)102, (char)136, (char)0, (char)70, (char)108, (char)105, (char)114, (char)116, (char)32, (char)14, (char)1103, (char)35, (char)55, (char)54, (char)34, (char)110, (char)98, (char)33, (char)76, (char)97, (char)32, (char)110, (char)115, (char)33, (char)114, (char)103, (char)33, (char)48, (char)56, (char)32, (char)66, (char)101, (char)11, (char)118, (char)34, (char)109, (char)108, (char)35, (char)99, (char)100, (char)35, (char)55, (char)51, (char)33, (char)66, (char)117, (char)32, (char)116, (char)114, (char)32, (char)119, (char)105, (char)31, (char)109, (char)109, (char)33, (char)115, (char)102, (char)54, (char)68, (char)105, (char)101, (char)32, (char)110, (char)116, (char)32, (char)108, (char)121, (char)33, (char)101, (char)109, (char)54, (char)116, (char)109, (char)117, (char)34, (char)111, (char)100, (char)34, (char)115, (char)100, (char)54, (char)75, (char)105, (char)115, (char)34, (char)68, (char)68, (char)16, (char)151, (char)17, (char)168, (char)17, (char)206, (char)15, (char)951, (char)35, (char)54, (char)56, (char)12, (char)81, (char)31, (char)103, (char)101, (char)35, (char)56, (char)52, (char)35, (char)70, (char)67, (char)34, (char)103, (char)115, (char)33, (char)82, (char)97, (char)33, (char)115, (char)117, (char)34, (char)105, (char)118, (char)35, (char)104, (char)104, (char)35, (char)80, (char)114, (char)7, (char)60, (char)31, (char)104, (char)97, (char)31, (char)111, (char)110, (char)10, (char)84, (char)52, (char)99, (char)104, (char)101, (char)8, (char)104, (char)7, (char)34, (char)255, (char)13, (char)12, (char)99, (char)108, (char)111, (char)117, (char)100, (char)115, (char)98, (char)108, (char)117, (char)101, (char)46, (char)103, (char)105, (char)102, (char)34, (char)67, (char)105, (char)13, (char)3, (char)55, (char)108, (char)105, (char)103, (char)14, (char)23, (char)16, (char)142, (char)16, (char)150, (char)18, (char)241, (char)18, (char)253, (char)18, (char)174, (char)18, (char)1091, (char)19, (char)942, (char)19, (char)304, (char)19, (char)322, (char)19, (char)324, (char)17, (char)961, (char)33, (char)98, (char)98, (char)53, (char)98, (char)101, (char)114, (char)32, (char)76, (char)105, (char)33, (char)68, (char)105, (char)13, (char)175, (char)34, (char)108, (char)103, (char)33, (char)65, (char)108, (char)54, (char)79, (char)117, (char)116, (char)32, (char)50, (char)52, (char)32, (char)117, (char)110, (char)32, (char)51, (char)57, (char)55, (char)117, (char)114, (char)116, (char)35, (char)73, (char)115, (char)35, (char)82, (char)102, (char)33, (char)55, (char)49, (char)33, (char)104, (char)108, (char)33, (char)66, (char)108, (char)32, (char)66, (char)97, (char)53, (char)101, (char)97, (char)114, (char)54, (char)101, (char)99, (char)104, (char)34, (char)85, (char)115, (char)34, (char)52, (char)55, (char)54, (char)105, (char)112, (char)105, (char)12, (char)59, (char)32, (char)122, (char)101, (char)7, (char)46, (char)9, (char)55, (char)31, (char)97, (char)116, (char)34, (char)119, (char)228, (char)35, (char)72, (char)117, (char)15, (char)8706, (char)16, (char)186, (char)17, (char)240, (char)17, (char)65273, (char)33, (char)223, (char)101, (char)33, (char)117, (char)116, (char)33, (char)102, (char)117, (char)73, (char)62, (char)45, (char)45, (char)60, (char)31, (char)97, (char)110, (char)32, (char)76, (char)101, (char)32, (char)115, (char)97, (char)13, (char)4, (char)34, (char)103, (char)116, (char)33, (char)106, (char)101, (char)53, (char)112, (char)119, (char)100, (char)32, (char)82, (char)101, (char)32, (char)104, (char)101, (char)32, (char)104, (char)111, (char)34, (char)111, (char)109, (char)34, (char)111, (char)107, (char)33, (char)53, (char)53, (char)52, (char)119, (char)97, (char)114, (char)53, (char)104, (char)101, (char)110, (char)34, (char)112, (char)101, (char)35, (char)101, (char)107, (char)35, (char)76, (char)76, (char)33, (char)98, (char)117, (char)31, (char)114, (char)101, (char)34, (char)119, (char)252, (char)34, (char)54, (char)54, (char)33, (char)83, (char)101, (char)11, (char)90, (char)33, (char)252, (char)99, (char)34, (char)67, (char)114, (char)34, (char)103, (char)117, (char)32, (char)122, (char)117, (char)12, (char)74, (char)34, (char)109, (char)228, (char)34, (char)52, (char)50, (char)33, (char)98, (char)111, (char)33, (char)67, (char)97, (char)31, (char)101, (char)115, (char)54, (char)84, (char)97, (char)103, (char)35, (char)82, (char)82, (char)35, (char)114, (char)121, (char)13, (char)5, (char)32, (char)50, (char)54, (char)34, (char)114, (char)114, (char)34, (char)111, (char)102, (char)54, (char)68, (char)111, (char)114, (char)53, (char)114, (char)101, (char)115, (char)53, (char)99, (char)103, (char)105, (char)33, (char)100, (char)100, (char)255, (char)57, (char)12, (char)124, (char)104, (char)116, (char)116, (char)112, (char)58, (char)47, (char)47, (char)119, (char)119, (char)119, (char)51, (char)46, (char)107, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)115, (char)46, (char)100, (char)101, (char)58, (char)56, (char)48, (char)56, (char)48, (char)47, (char)116, (char)120, (char)116, (char)108, (char)47, (char)99, (char)108, (char)105, (char)99, (char)107, (char)63, (char)100, (char)61, (char)107, (char)110, (char)117, (char)100, (char)100, (char)101, (char)108, (char)115, (char)46, (char)100, (char)101, (char)38, (char)105, (char)100, (char)61, (char)33, (char)83, (char)109, (char)34, (char)52, (char)51, (char)34, (char)97, (char)102, (char)53, (char)105, (char)110, (char)101, (char)33, (char)67, (char)111, (char)18, (char)170, (char)18, (char)179, (char)17, (char)931, (char)16, (char)137, (char)15, (char)187, (char)35, (char)67, (char)69, (char)34, (char)108, (char)107, (char)32, (char)97, (char)109, (char)32, (char)112, (char)112, (char)33, (char)115, (char)119, (char)35, (char)54, (char)51, (char)35, (char)109, (char)121, (char)35, (char)112, (char)102, (char)17, (char)156, (char)17, (char)230, (char)16, (char)140, (char)15, (char)28, (char)54, (char)102, (char)104, (char)101, (char)13, (char)8, (char)34, (char)86, (char)105, (char)53, (char)116, (char)101, (char)114, (char)10, (char)255, (char)31, (char)49, (char)49, (char)33, (char)102, (char)97, (char)35, (char)115, (char)114, (char)35, (char)77, (char)102, (char)13, (char)16, (char)96, (char)176, (char)62, (char)115, (char)109, (char)95, (char)54, (char)65, (char)110, (char)122, (char)10, (char)65, (char)32, (char)97, (char)115, (char)35, (char)78, (char)82, (char)35, (char)108, (char)114, (char)35, (char)78, (char)84, (char)35, (char)54, (char)49, (char)33, (char)110, (char)122, (char)33, (char)97, (char)98, (char)33, (char)103, (char)105, (char)54, (char)100, (char)112, (char)103, (char)33, (char)103, (char)114, (char)9, (char)40, (char)255, (char)12, (char)10, (char)112, (char)105, (char)99, (char)115, (char)47, (char)109, (char)97, (char)108, (char)101, (char)46, (char)103, (char)105, (char)102, (char)32, (char)112, (char)103, (char)35, (char)67, (char)117, (char)19, (char)948, (char)19, (char)962, (char)19, (char)1754, (char)19, (char)8596, (char)18, (char)1110, (char)18, (char)9658, (char)17, (char)1750, (char)19, (char)273, (char)19, (char)295, (char)19, (char)423, (char)19, (char)1753, (char)18, (char)957, (char)18, (char)1089, (char)17, (char)207, (char)17, (char)237, (char)18, (char)9679, (char)18, (char)8221, (char)34, (char)79, (char)115, (char)33, (char)111, (char)119, (char)31, (char)99, (char)107, (char)11, (char)229, (char)32, (char)72, (char)97, (char)35, (char)103, (char)100, (char)35, (char)73, (char)116, (char)34, (char)107, (char)116, (char)54, (char)108, (char)101, (char)110, (char)33, (char)69, (char)105, (char)13, (char)7, (char)34, (char)102, (char)114, (char)10, (char)67, (char)9, (char)41, (char)54, (char)109, (char)105, (char)110, (char)54, (char)83, (char)105, (char)101, (char)12, (char)123, (char)34, (char)54, (char)53, (char)34, (char)77, (char)252, (char)11, (char)246, (char)35, (char)78, (char)252, (char)35, (char)68, (char)111, (char)34, (char)105, (char)111, (char)33, (char)80, (char)102, (char)54, (char)100, (char)97, (char)115, (char)12, (char)86, (char)95, (char)70, (char)108, (char)105, (char)114, (char)116, (char)11, (char)252, (char)34, (char)114, (char)122, (char)35, (char)71, (char)117, (char)35, (char)102, (char)109, (char)54, (char)72, (char)97, (char)109, (char)8, (char)116, (char)29, (char)101, (char)110, (char)29, (char)101, (char)114, (char)4, (char)0, (char)52, (char)100, (char)101, (char)114, (char)33, (char)67, (char)99, (char)54, (char)65, (char)108, (char)116, (char)53, (char)77, (char)105, (char)120, (char)35, (char)65, (char)115, (char)35, (char)116, (char)112, (char)55, (char)110, (char)101, (char)110, (char)33, (char)108, (char)100, (char)32, (char)48, (char)53, (char)31, (char)105, (char)115, (char)11, (char)195, (char)34, (char)76, (char)252, (char)15, (char)1752, (char)15, (char)1761, (char)15, (char)1762, (char)15, (char)162, (char)54, (char)71, (char)101, (char)115, (char)54, (char)97, (char)117, (char)102, (char)54, (char)97, (char)110, (char)100, (char)54, (char)98, (char)108, (char)103, (char)33, (char)112, (char)114, (char)52, (char)117, (char)110, (char)100, (char)31, (char)97, (char)108, (char)34, (char)73, (char)110, (char)34, (char)87, (char)252, (char)54, (char)104, (char)105, (char)101, (char)54, (char)103, (char)101, (char)115, (char)34, (char)246, (char)223, (char)34, (char)112, (char)115, (char)31, (char)98, (char)101, (char)32, (char)50, (char)53, (char)54, (char)49, (char)52, (char)48, (char)35, (char)104, (char)115, (char)16, (char)969, (char)16, (char)239, (char)16, (char)147, (char)16, (char)963, (char)34, (char)52, (char)54, (char)10, (char)128, (char)31, (char)49, (char)52, (char)32, (char)51, (char)53, (char)33, (char)69, (char)109, (char)54, (char)103, (char)101, (char)114, (char)31, (char)99, (char)103, (char)10, (char)107, (char)6, (char)45, (char)34, (char)119, (char)104, (char)35, (char)114, (char)112, (char)17, (char)148, (char)18, (char)923, (char)18, (char)155, (char)16, (char)9553, (char)16, (char)129, (char)17, (char)208, (char)19, (char)287, (char)19, (char)399, (char)19, (char)9632, (char)19, (char)9668, (char)33, (char)80, (char)97, (char)11, (char)85, (char)32, (char)98, (char)108, (char)33, (char)99, (char)97, (char)54, (char)118, (char)97, (char)116, (char)33, (char)51, (char)55, (char)34, (char)101, (char)117, (char)34, (char)109, (char)115, (char)33, (char)77, (char)105, (char)33, (char)99, (char)115, (char)53, (char)100, (char)105, (char)101, (char)33, (char)97, (char)121, (char)33, (char)102, (char)105, (char)10, (char)192, (char)32, (char)117, (char)109, (char)53, (char)115, (char)101, (char)110, (char)9, (char)61, (char)33, (char)66, (char)114, (char)34, (char)117, (char)108, (char)34, (char)102, (char)121, (char)53, (char)118, (char)111, (char)114, (char)33, (char)87, (char)108, (char)54, (char)116, (char)115, (char)99, (char)32, (char)114, (char)100, (char)31, (char)110, (char)101, (char)34, (char)75, (char)117, (char)15, (char)949, (char)15, (char)1090, (char)35, (char)82, (char)252, (char)54, (char)80, (char)97, (char)114, (char)35, (char)56, (char)53, (char)35, (char)119, (char)99, (char)34, (char)99, (char)114, (char)33, (char)114, (char)109, (char)8, (char)49, (char)6, (char)95, (char)4, (char)32 }))
        { }
        /// <summary>
        /// Erstellt eine neue Instanz der Huffman-Komprimierung anhand des Tree-String.
        /// </summary>
        /// <param name="pTree">Der zu verarbeitende Tree-String</param>
        public Huffman(string pTree)
        {
            this._tree = new Dictionary<string, string>(); // erstellen einer neuen Key, Value Liste für eine einfache verwendung der Tree Werte
            this._helper = new StringBuilder(); // _helper" definieren um ihnals "bitBuffer" zu verwenden (Key)

            var pathIndex = 1; // "pathIndex" definieren, gibt den anganh des Weg´s im Tree an
            var treeDepth = -33; // "treeDepth" definieren, gibt die Tiefe im Tree an
            int valueLength; // "valueLength" definieren, gibt die Länge des String Werts an (Value)
            for (var index = 0; index < pTree.Length; index += valueLength + 1) // gehe jedes zeichen im Tree ("pTree") durch und addiere errechnete "valueLength" + 1 zum index um zum nächsten Key/Value Part zu gelangen
            {
                var c = (int)pTree[index]; // Diese char gibt an um wie die Länge des Paths und des Werts berechnet werden müssen
                int pathLength;
                if (c == 255)
                {
                    valueLength = (int)pTree[index + 1] + 1; // "valueLength" (Die Länge des String Werts im Tree) auslesen
                    pathLength = (int)pTree[index + 2]; // "pathLength" (Die Länge des Path Werts im Tree) auslesen
                    index += 2; // index um 2 addieren da wir 2 chars ausgelesen haben
                }
                else
                {
                    valueLength = c / 21 + 1; // "valueLength" (Die Länge des String Werts im Tree) anhand des chars ("c") berechnen
                    pathLength = c % 21; // "pathLength" (Die Länge des Path Werts im Tree) anhand des chars ("c") berechnen
                }

                if ((pathIndex & 1) == 0) // wenn "pathIndex" & 1 nicht gleich '0'
                {
                    ++pathIndex; // "pathIndex" um 1 addieren
                    for (; treeDepth < pathLength; ++treeDepth) // solange "treeDEpzh" kleiner als "pathLength", "treeDEpth" um 1 addieren...
                        pathIndex <<= 1; // ...und den "pathIndex" um 1 nach links verschieben
                }
                else // wenn "pathIndex" & 1 nicht gleich '1'
                {
                    do // "treeDepth" und "pathIndex" berechnen solange...
                    {
                        pathIndex >>= 1; // "treeIndex" um 1 nach rechts verschieben
                        --treeDepth; // "treeDepth" um 1 subtrahieren
                    }
                    while ((pathIndex & 1) == 1); // ..."pathIndex" & 1 gleich 1 sind.
                    ++pathIndex; // "pathIndex" um 1 addieren
                    for (; treeDepth < pathLength; ++treeDepth) // arbeite solange "treeDepth" kleiner als "pathLength"
                        pathIndex <<= 1; // "pathIndex" um 1 nach links verschieben
                }

                int path = CalcPath(pathIndex, pathLength); // "path" berechnen (wird für den Key Wert gebraucht)
                string value = pTree.Substring(index + 1, valueLength); // String Wert für "bitBuffer" (Key) aus dem Tree entnehmen

                this._helper.Clear(); // buffer leeren um ihn erneut als "bitBuffer" zu verwenden
                do
                { // arbeite so lange wie...
                    this._helper.Append(path & 1); // "bitWert" mithilfe des "path" berechnen (0 oder 1)

                    path >>= 1; // "path" um eine Position nach rechts verschieben
                    pathLength--; // länge um 1 subtrahieren
                } while (pathLength != 0); // ..."length" (Länge) nicht 0 ist. 

                if (value == "\\\\\\") // 16 bit char indikator "\\\" (ist von Knuddels so vorgegeben)
                {
                    this._16BitCharIndicator = _helper.ToString(); // "bitBuffer" als "_16BitCharIndicator" festlegen um ihn soäter für die Komprimierung von 16 Bit Zeichen zu verwendne
                }
                this._helper.Append("1"); // Bit Ende hinzufügen

                if (this._tree.ContainsKey(this._helper.ToString())) // sollte der Tree bereits diesen Key enthalten Error werfen (Tree fehlerhaft)
                    throw new Exception(string.Format("Error constructing tree (Key: {0}, Value: {1}, Path: {2}, PathLength: {3})", this._helper.ToString(), value, path, pathLength));

                Debug.WriteLine(_helper + " - " + value.Replace("\0", "\\0").Replace("\n", "\\n"));
                this._tree.Add(this._helper.ToString(), value); // errechneten Werte zum Tree hinzufügen
                this._helper.Clear();
            }
        }

        /// <summary>
        /// Berechnet einen Path für einen Knoten im Tree.
        /// </summary>
        /// <param name="pathIndex">Der Start-Index des Paths</param>
        /// <param name="pathLength">Die Länge des Paths</param>
        /// <returns></returns>
        private int CalcPath(int pathIndex, int pathLength)
        {
            int path = 0; // berechneten Wert definieren
            int shiftHelper = 1; // nach links zu verschiebende länge definieren
            int pathCounter = 1 << pathLength - 1; // zu addierende Zahl definieren
            for (; pathLength > 0; --pathLength)
            { // zu addierende zahl berechnen
                if ((pathIndex & shiftHelper) != 0) // sollte die "zu addierende Zahl" (pathCounter) & "shiftHelper" nicht '0' sein dann...
                    path += pathCounter; // "pathCounter" zu rückgabe ("path" (berechneten Wert) addieren
                shiftHelper <<= 1; // "shiftHelper" um eine Position nach links verschieben
                pathCounter >>= 1; // "pathCounter" um eine Position nach rechts verschieben
            }
            return path; // berechneten Wert zurückgeben
        }

        #endregion

        /* Values berechnen um es wie Knuddels zu Handeln (benötigt viele änderungen am Code), hatte ich aber bisher keine 
         * Lust den Unterschied zu testen, aber da beides auf der Dictionary basiert sollte da kein großartiger Unterschied 
         * entstehen.
         *  Index:  
         *          Decompress:
         *              Convert.ToInt32(_helper.Substring(0, _helper.Length - 1).ToString(), 2)
         *          Compress:
         *              Convert.ToInt32(pair.Key.Substring(0, pair.Key.Length - 1), 2)
         * 
         *  Length: 
         *          Decompress:
         *              _helper.Length - 1  | -1 weil 1 beim erstellen des Trees als offset hinzugefügt wird
         *          Compress:
         *              pair.Key.Length - 1 | -1 weil 1 beim erstellen des Trees als offset hinzugefügt wird
         * 
         *  Path:   CalcPath(index, length)
         */

        #region Compress

        /// <summary>
        /// Komprimiert einen Text in ein Byte Array. (Huddels-Huffman)
        /// </summary>
        /// <param name="pString">Der zu komprimierende Text</param>
        /// <returns>Das komprimierte Byte-Array</returns>
        public byte[] Compress(string pString)
        {
            if (pString == null) // sollte eingabe gleich "null"
                pString = string.Empty; // eingabe einen leeren string zuweisen

            lock (this._helper) // den hilf buffer blockieren das keine anderen daten hinzugefügt werden können
            {
                this._helper.Clear(); // temp buffer leeren um ihn als "charBuffer" zu verwenden

                var bitBuffer = new StringBuilder(); // erstellen eines StringBuilders worin die bit segmente gespeichert werden

                for (var index = 0; index < pString.Length; ++index) //alle zeichen von "pString" durchgehen
                {
                    this._helper.Append(pString[index]); // zeichen zum "charBuffer" bufer hinzufügen
                    if (_tree.ContainsValue(this._helper.ToString())) //prüfen ob die zeichenkette im tree existiert
                    {
                        foreach (var charPair in _tree) // alle werte im tree durchgehen
                        {
                            if (charPair.Value == this._helper.ToString())    // wenn die temp zeichenkette im tree enthalten ist
                            {
                                // Suche zusätzlich nach Zeichenketten (Beispiel: J, Ja, Jam, Jame, James)
                                for (index += 1; index < pString.Length; ++index) // starte die suche bei der Position wo das Zeichen gefunden wurde + 1 (nächstes Zeichen)
                                {
                                    this._helper.Append(pString[index]); // zeichen zum "charBuffer" bufer hinzufügen

                                    if (!_tree.ContainsValue(this._helper.ToString()) && (index + 1 < pString.Length))
                                    {
                                        index++; // gehe eine Position im Eingabe Stream (pString) weiter um zum nächsten Zeichen zu gelangen
                                        this._helper.Append(pString[index]); // zeichen zum "charBuffer" bufer hinzufügen
                                        if (!_tree.ContainsValue(this._helper.ToString()))
                                        { // zeichenkette existiert nicht
                                            this._helper.Remove(_helper.Length - 1, 1); // lösche zuletzt hinzugefügtes Zeichen
                                            index--; // gehe eine Position im Eingabe Stream (pString) zurück
                                        }
                                    }

                                    if (!_tree.ContainsValue(this._helper.ToString()))
                                    { // zeichenkette existiert nicht
                                        this._helper.Remove(_helper.Length - 1, 1); // lösche zuletzt hinzugefügtes Zeichen
                                        index--; // gehe eine Position im Eingabe Stream (pString) zurück
                                        break; // Zeichenketten suche untergrechen da sie offenbar nicht existiert
                                    }
                                }

                                foreach (var pair in _tree) // gehe alle Werte im Tree durch
                                {
                                    if (pair.Value == _helper.ToString()) // Wenn die Zeichenkette im Tree gefunden wird 
                                    {
                                        bitBuffer.Append(pair.Key.Substring(0, pair.Key.Length - 1)); // die bit werte der gefunden kette dem bitBuffer hinzufügen
                                        this._helper.Clear(); //den temp buffer zur weiter verwenung leeren
                                        break;  // temp zeichen gefunden suchen beenden
                                    }
                                }
                                break; // temp zeichen kette kette gefunden suchen beenden
                            }
                        }
                        this._helper.Clear();
                    }
                    else // 16-Bit zeichen
                    {   //zeichen welche nicht im tree sind (z.b Short Werte aus dem GenericProtocol oder Zeichen wie 💛)
                        bitBuffer.Append(this._16BitCharIndicator); // 16-bit char indikator welche aus dem tree ausgelesen wurde
                        this._helper.Clear(); // temp buffer leeren um ihn als "charBuffer" verwenden zu können

                        for (int bitCounter = 0; bitCounter < 16; bitCounter++) // 16 bit durchgehen
                        {
                            this._helper.Append(((pString[index]) >> bitCounter) & 1); // bit zeichen ( 0 oder 1 berechnen)
                        }
                        bitBuffer.Append(this._helper); // "charBuffer" den haupt buffer hinzufügen
                        this._helper.Clear(); // "charBuffer" leeren
                    }
                }
                this._helper.Clear();
                // den bit stream in  ein byte stream umwandeln
                var buffer = new List<byte>(); //  //byte buffer erstellen
                
                var bits = new string(bitBuffer.ToString().Reverse().ToArray()); // die bitwerte umkeheren
                for (int index = bits.Length; index > 0; index -= index < 8 ? index : 8) // 8 bit zum "index" hinzufügen wenn verfügbar andernfalls die länge der verbleibenden zeichen
                { // alle bits rückwerts durchgehen
                    buffer.Add(Convert.ToByte( // bit segment in ein byte umwandeln(1 byte = 8 bits)
                                        bits.Substring( // bit segment aus dem bitStream entnehemen
                                            index - 8 < 0 ? 0 : index - 8, // start index berechnen, sollte dieser kleiner als 0 sein dann 0 andernfalls werden 8 bit vom "index" abgezogen
                                            index < 8 ? index : 8 // end index berechnen, sollte dieser kleiner als 8 (ein bit) sein werden alle verbleibenden zeichen genutzt
                                        ),
                                        2 //basis der zahl
                                    ));
                }
                bitBuffer.Clear();
                return buffer.ToArray(); // "ausgabe buffer" zurückgeben
            }
        }

        private string FindKey(string pValue, ref int pIndex, out bool end)
        {
            for (var index = pIndex; pIndex < pValue.Length; ++pIndex)
            {
                _helper.Append(pValue[pIndex++]);
                do
                {
                    if (pIndex >= pValue.Length) break;

                    _helper.Append(pValue[pIndex++]);
                } while (!_tree.ContainsValue(_helper.ToString()));

                do //Search for Full Words
                {
                    if (pIndex >= pValue.Length) break;

                    _helper.Append(pValue[pIndex++]);
                } while (!_tree.ContainsValue(_helper.ToString()));

                if (!_tree.ContainsValue(_helper.ToString()))
                {
                    pIndex = index;
                    _helper.Clear().Append(pValue[pIndex++]);
                }

                end = pIndex == pValue.Length;
                foreach (var pair in _tree)
                    if (pair.Value == _helper.ToString())
                        return pair.Key;
            }
            end = pIndex >= pValue.Length;
            return null;
        }


        #endregion

        #region Decompress

        /// <summary>
        /// Dekomprimiert ein Byte-Array (Knuddels-Huffman) wieder in einen lesbaren Text.
        /// </summary>
        /// <param name="pBuffer">Das komprimierte Byte-Array</param>
        /// <returns>Der Entkomprimierte Text</returns>
        public string Decompress(byte[] pBuffer)
        {
            if (pBuffer == null) // sollte eingabe ("pBuffer") gleich "null" sein
                return string.Empty; // leeren String zurückgeben

            lock (_helper) // den hilf buffer blockieren das keine anderen daten hinzugefügt werden können
            {
                this._helper.Clear(); //// "bitValue" leeren um ihn erneut zu verwenden 
                var buffer = new StringBuilder(); // "ausgabe buffer" erstellen
              
                var end = false; // boolean Wert definieren der angibt ob das Ende von der Eingabe ("pBuffer") erreicht wurde
                var index = 0; // int Wert deiniere der die Position in der Eingabe ("pBuffer") angibt
                var bitIndex = 0; // int Wert definieren welcher den index der Bits in den Bytes angibt

                while (!end) // solange arbeiten bis das ende vom input ("pBuffer") erreicht wurde
                {
                    _helper.Append(GetBitValue(pBuffer, ref index, ref bitIndex, ref end)); // bit wert berechnen und "bitValue" hinzufügen
                    _helper.Append("1"); // bit ende temporär hinzufügen

                    if (_tree.ContainsKey(_helper.ToString()))
                    { // "bitValue" existiert im tree
                        var value = _tree[_helper.ToString()]; // wert für "bitValue" aus dem tree entnahmen
                        if (value == "\\\\\\") // wenn "value" gleich '\\\' (16 bit indikator string)
                        { // muss das originale char berechnet werden
                            var charValue = 0;
                            for (int j = 0; j < 16; ++j) // 16 bits durchgehen
                            {
                                charValue += (GetBitValue(pBuffer, ref index, ref bitIndex, ref end) << j); //  bit Wert berechnen und um "j" stellen nach links verschieben anschließend zur "charValue" addieren
                            }
                            buffer.Append((char)charValue); // die berechnete zahl in ein char umwandeln und zum "ausgabe buffer" hinzufügen
                        }
                        else
                        { // "bitValue" existiert im tree
                            buffer.Append(value); // den wert der "bitValue" aus dem tree zum "ausgabe buffer" hinzufügen
                        }
                        _helper.Clear(); // "bitValue" leeren um ihn erneut zu verwenden
                    }
                    else
                    { // "bitValue" existiert nicht im tree
                        this._helper.Remove(_helper.Length - 1, 1); // bit ende wieder von "bitValue" entfernen
                    }
                }

                return buffer.ToString(); // "ausgabe buffer" zurückgeben
            }
        }

        /// <summary>
        /// Berechnet den Bit-Wert des angegebenen Bytes
        /// </summary>
        /// <param name="buffer">Das Input Byte-Array</param>
        /// <param name="index">Die Position des zu verwendenden Bytes</param>
        /// <param name="bitIndex">Der aktuelle Bit Index im zu verwendenden Byte</param>
        /// <param name="end"></param>
        /// <returns></returns>
        private int GetBitValue(byte[] buffer, ref int index, ref int bitIndex, ref bool end)
        {
            int bitValue = 0; // rückgabe variable definieren (Default: 0)
            if (((int)buffer[index] & 1 << bitIndex) != 0) // wenn das byte aus der eingabe ("pBuffer") der Position ("index") & 1 um "bitIndex" nach links verschoben nicht '0' ist...
                bitValue = 1; // ...dann rückgabe wert auf '1' festlegen
            ++bitIndex; // "bitIndex" um 1 addieren
            if (bitIndex > 7) // wenn "bitIndex" gleich 8 (ein byte)
            {
                bitIndex = 0; // "bitIndex" zurücksetzen
                ++index; // "index" (Position in der Eingabe ("pBuffer") um 1 addieren
                end = index == buffer.Length; // ist der "index" gleich die Länge der Eingabe  ("pBuffer") ist der vorgang beendet
            }
            return bitValue; // "bitValue" (0 oder 1) zurückgeben
        }

        #endregion
    }
}