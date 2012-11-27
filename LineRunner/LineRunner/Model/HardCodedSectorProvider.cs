using System.Collections.Generic;
using System;

namespace LineRunner.Model
{
    public class HardCodedSectorProvider : ISectorProvider
    {
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

            /*
          this.AddSector(
                "                                                                                                ",
                " I      II      III      IIII      IIIII      IIIIII      IIIIIII      IIIIIIII      IIIIIIII   "); 

            this.AddSector(
                "                                                                           ",
                "  I       I       I      I      I     I     I    I    I   I   I  I  I I I  "); */
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


            // Ei ihan alkuun tai loppuun palikoita


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




            // hienoi

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
