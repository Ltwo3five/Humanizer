﻿namespace Humanizer
{
    class PolishNumberToWordsConverter(CultureInfo? culture) :
        GenderedNumberToWordsConverter
    {
        static readonly string[] HundredsMap =
        [
            "zero", "sto", "dwieście", "trzysta", "czterysta", "pięćset", "sześćset", "siedemset", "osiemset", "dziewięćset"
        ];

        static readonly string[] TensMap =
        [
            "zero", "dziesięć", "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", "sześćdziesiąt",
            "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt"
        ];

        static readonly string[] UnitsMap =
        [
            "zero", "jeden", "dwa", "trzy", "cztery", "pięć", "sześć", "siedem", "osiem", "dziewięć", "dziesięć",
            "jedenaście", "dwanaście", "trzynaście", "czternaście", "piętnaście", "szesnaście", "siedemnaście",
            "osiemnaście", "dziewiętnaście"
        ];

        static readonly string[][] PowersOfThousandMap =
        [
            ["tysiąc", "tysiące", "tysięcy"],
            ["milion", "miliony", "milionów"],
            ["miliard", "miliardy", "miliardów"],
            ["bilion", "biliony", "bilionów"],
            ["biliard", "biliardy", "biliardów"],
            ["trylion", "tryliony", "trylionów"]
        ];

        const long MaxPossibleDivisor = 1_000_000_000_000_000_000;

        public override string Convert(long input, GrammaticalGender gender, bool addAnd = true)
        {
            if (input == 0)
            {
                return "zero";
            }

            var parts = new List<string>();
            CollectParts(parts, input, gender);

            return string.Join(" ", parts);
        }

        public override string ConvertToOrdinal(int number, GrammaticalGender gender) =>
            number.ToString(culture);

        static void CollectParts(ICollection<string> parts, long input, GrammaticalGender gender)
        {
            var inputSign = 1;
            if (input < 0)
            {
                parts.Add("minus");
                inputSign = -1;
            }

            var number = input;
            var divisor = MaxPossibleDivisor;
            var power = PowersOfThousandMap.Length - 1;
            while (divisor > 0)
            {
                var multiplier = (int) Math.Abs(number / divisor);
                if (divisor > 1)
                {
                    if (multiplier > 1)
                    {
                        CollectPartsUnderThousand(parts, multiplier, GrammaticalGender.Masculine);
                    }

                    if (multiplier > 0)
                    {
                        parts.Add(GetPowerOfThousandNameForm(multiplier, power));
                    }
                }
                else if (multiplier > 0)
                {
                    if (multiplier == 1 && Math.Abs(input) != 1)
                    {
                        gender = GrammaticalGender.Masculine;
                    }
                    CollectPartsUnderThousand(parts, multiplier, gender);
                }

                number -= multiplier * divisor * inputSign;
                divisor /= 1000;
                power--;
            }
        }

        static void CollectPartsUnderThousand(ICollection<string> parts, int number, GrammaticalGender gender)
        {
            var hundredsDigit = number / 100;
            var tensDigit = number % 100 / 10;
            var unitsDigit = number % 10;

            if (hundredsDigit >= 1)
            {
                parts.Add(HundredsMap[hundredsDigit]);
            }

            if (tensDigit >= 2)
            {
                parts.Add(TensMap[tensDigit]);
            }

            if (tensDigit != 1 && unitsDigit == 2)
            {
                var genderedForm = gender == GrammaticalGender.Feminine ? "dwie" : "dwa";
                parts.Add(genderedForm);
            }
            else if (number == 1)
            {
                var genderedForm = gender switch
                {
                    GrammaticalGender.Masculine => "jeden",
                    GrammaticalGender.Feminine => "jedna",
                    GrammaticalGender.Neuter => "jedno",
                    _ => throw new ArgumentOutOfRangeException(nameof(gender))
                };
                parts.Add(genderedForm);
            }
            else
            {
                var unit = unitsDigit + 10 * (tensDigit == 1 ? 1 : 0);
                if (unit > 0)
                {
                    parts.Add(UnitsMap[unit]);
                }
            }
        }

        static string GetPowerOfThousandNameForm(int multiplier, int power)
        {
            const int singularIndex = 0;
            const int pluralIndex = 1;
            const int genitiveIndex = 2;
            if (multiplier == 1)
            {
                return PowersOfThousandMap[power][singularIndex];
            }

            var multiplierUnitsDigit = multiplier % 10;
            var multiplierTensDigit = multiplier % 100 / 10;
            if (multiplierTensDigit == 1 || multiplierUnitsDigit <= 1 || multiplierUnitsDigit >= 5)
            {
                return PowersOfThousandMap[power][genitiveIndex];
            }
            return PowersOfThousandMap[power][pluralIndex];
        }
    }
}
