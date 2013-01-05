using Flai;
using System;
using System.Collections.Generic;

namespace LineRunner.Model
{
    public class HardCodedSectorProvider : ISectorProvider
    {
        private const int SuperEasySectorCount = 3;
        private const int EasySectorCount = 16;

        public static readonly RangeInt SuperEasySectors = new RangeInt(1, 1 + SuperEasySectorCount);
        public static readonly RangeInt EasySectors = new RangeInt(SuperEasySectors.Max, SuperEasySectors.Max + EasySectorCount);

        private List<Sector> _sectors = new List<Sector>();

        // RULE 1: There can't be anything on upperLevel before 4-5 block before the end
        // RULE 2: Only 3 tiles in row at maximum
        public HardCodedSectorProvider()
        {
            #region Old
            // First sector is empty
            this.AddSector(
                new string(' ', 14),
                new string(' ', 14));

            #region Super Helpot

            this.AddSector(
                "                                                                                    ",
                "      I           I                II                I                  II          ");

            this.AddSector(
                "         -             --                         --          ",
                "                                     II                       ");

            this.AddSector(
                "                       -                          --                          --             ",
                "          I                          II                         II                           ");

            #endregion

            #region Helpot ( 16 )

            this.AddSector(
                "                          -                    --                   ",
                "      I         I                   I                      II       ");

            this.AddSector(
                "             -                       ",
                "    II                I        I     ");

            this.AddSector(
                "                 -                     --                  -       ",
                "      I                    II                    I                 ");

            this.AddSector(
                "                                   -                 ",
                "      I         I         II                  I      ");

            this.AddSector(
                "                      --                  -              ",
                "      I       I                 II                 I     ");

            this.AddSector(
                "           -           --                           --      ",
                "    I                             I        I                ");

            this.AddSector(
                "                                              ",
                "     I         II          II        I        ");

            this.AddSector(
                "            -                       ",
                "   I                III        I    ");

            this.AddSector(
                "                         -                        ",
                "      I        II                I         I      ");

            this.AddSector(
                "                      -                     ",
                "      II       I             II        I    ");

            this.AddSector(
                "                        --                         ",
                "      I        II                 I         II     ");

            this.AddSector(
                "                         -                         ",
                "      II        I                 II         I     ");

            this.AddSector(
                "       --                               -                     ",
                "               I       I       II               I      II     ");

            this.AddSector(
                "                             -                               ",
                "      I       I       II            II         I       I     ");

            this.AddSector(
                "                                                  -             ",
                "      II        I       I      II       II                I     ");

            this.AddSector(
                "                                           -           ",
                "      II        I        III       I              I    ");

            #endregion

            // Testaa pelk‰st‰‰n noit mun tekemi‰, et jos siel on jotai mahottomia.
            this.AddSector(
                 "                -       ",
                 "   II     I         I   ");

            this.AddSector(
                "   -                       -            ",
                "          II        I I        I I      ");

            this.AddSector(
                "                -         ",
                " I        I          I    ");

            this.AddSector(
                "   -           - -      ",
                "        II              ");

            this.AddSector(
                "     ",
                "     ");

            this.AddSector(
                "     -         ",
                "           I   ");

            this.AddSector(
                "   --                 ",
                "          II     I    ");

            this.AddSector(
                "                   -                ",
                "     I         I       II       I   ");

            this.AddSector(
                "   ---                    ",
                "              III     I   ");

            this.AddSector(
                "       ---        --         -          ",
                "   I         II        III        I I   ");


            // Antin

            this.AddSector(
                "         --        -            ---           ",
                "   II           I        II              I    ");

            this.AddSector(
                "         --                -        -   ",
                "   I           I      I         I       ");


            this.AddSector(
                "       -          -        ",
                "  I         II         I   ");

            this.AddSector(
                "       --                 -  ",
                "  I           II     I       ");

            this.AddSector(
                "         -        --        ---       ",
                "   III       II         I         I   ");

            this.AddSector(
                "        --          --         -   ",
                "   I          II          I        ");

            this.AddSector(
                "       -                         --    ",
                "  II         III     I     III         ");

            this.AddSector(
                "      -           -           --   ",
                "  I        III         III         ");

            this.AddSector(
                "              --        ---         -    ",
                "  II     I          I          I         ");

            this.AddSector(
                "   -          -         -             --   ",
                "       II          I          I  I         ");

            this.AddSector(
                "        -           -           --    ",
                "   I         III         III          ");

            this.AddSector(
                "        -          -           -         -        -   ",
                "  II         I           II         I         I       ");

            this.AddSector(
                "      --           --            --        ",
                "  I          II           I I          II  ");

            this.AddSector(
                "       --          --            --          ",
                "   I         II           II            II   ");

            this.AddSector(
                "        --         --          --            -     ",
                "  I           I          I            II           ");

            this.AddSector(
                "        -         -           --           -          -          ",
                "   I        II         III          III         II          I    ");

            this.AddSector(
                "          -           --          --          ---          --          -    ",
                "  I I           I            I          II            I           I         ");

            this.AddSector(
                "       -          -           --           --    ",
                "   I       III          II           I           ");

            this.AddSector(
                "         -           --             --           -    ",
                "   II         II             III            I         ");

            this.AddSector(
                "       -           -            --              ---     ",
                "   I         I          I                I              ");
            this.AddSector(
                "          --   ",
                "    I          ");

            // Hienot

            this.AddSector(
                "                                                            ",
                "      I       I      I     I    I     I      I       I      ");
            this.AddSector(
                "                                         ---                                       ",
                "      I      II      IIII      IIII               IIII      III      II      I     ");
            this.AddSector(
                "                         ",
                "          IIIII          ");
            this.AddSector(
                "         -            --             ---             ---            --           -     ",
                "   I           II            III             III             II            I           ");
            
            #endregion
        }

        private void AddSector(string upperLevel, string groundLevel)
        {
            if (upperLevel.Length != groundLevel.Length)
            {
                throw new ArgumentException("Upper level and ground level must have same length");
            }

            _sectors.Add(Sector.FromString(
                upperLevel,
                groundLevel));
        }

        #region ISectorProvider Members

        public int SectorCount
        {
            get { return _sectors.Count; }
        }

        public Sector this[int i]
        {
            get { return _sectors[i]; }
        }

        #endregion
    }
}
