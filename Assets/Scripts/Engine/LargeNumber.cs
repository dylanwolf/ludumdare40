using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LargeNumber : IComparable<LargeNumber>, IEquatable<LargeNumber>, IComparable<long>, IEquatable<long>
{
    static readonly string[] Suffixes = new string[] { string.Empty, "k", "M", "G", "T", "P", "E", "Z", "Y" };
    public long[] Placeholders;
    public byte NumberOfPlaces;

    public LargeNumber(long? number = null)
    {
        Placeholders = new long[9];
        if (number.HasValue)
            SetValue(number.Value);
        else
            Reset();
    }

    public void SetValue(long number)
    {
        Reset();
        NumberOfPlaces = 0;
        while (number > 0)
        {
            Placeholders[NumberOfPlaces] = (long)(number % 1000);
            number = (long)Math.Floor(number / 1000.0);
            NumberOfPlaces++;
        }

        if (NumberOfPlaces < 1)
            NumberOfPlaces = 1;
    }

    public void Reset()
    {
        for (byte i = 0; i < Placeholders.Length; i++)
            Placeholders[i] = 0;
        NumberOfPlaces = 1;
    }

    const string SEPARATOR = ",";

    public string ToFullString()
    {
        var fullString = string.Join(SEPARATOR, Placeholders.Take(NumberOfPlaces).Reverse().Select(x => x.ToString("000")).ToArray());
        while (fullString.StartsWith("0") || fullString.StartsWith(SEPARATOR))
            fullString = fullString.Substring(1);
        return fullString;
    }

    public override string ToString()
    {
        return ToFullString();
    }

    string _lastShortString;
    byte? _lastShortStringPlace;
    decimal? _lastShortStringSignificantValue;
    public string ToShortString()
    {
        if (!_lastShortStringPlace.HasValue || !_lastShortStringSignificantValue.HasValue ||
            _lastShortStringPlace.Value != NumberOfPlaces || _lastShortStringSignificantValue.Value != Placeholders[NumberOfPlaces - 1])
        {
            _lastShortStringSignificantValue = (NumberOfPlaces <= 1) ? Placeholders[NumberOfPlaces - 1] : (Placeholders[NumberOfPlaces - 1] + (Placeholders[NumberOfPlaces - 2] / 1000m));

            _lastShortString = string.Format("{0:0.#}{1}", _lastShortStringSignificantValue, Suffixes[NumberOfPlaces - 1]);
            _lastShortStringPlace = NumberOfPlaces;
        }

        return _lastShortString;
    }

    public int CompareTo(LargeNumber n)
    {
        if (NumberOfPlaces != n.NumberOfPlaces)
            return NumberOfPlaces.CompareTo(n.NumberOfPlaces);

        for (int i = NumberOfPlaces - 1; i >= 0; i--)
        {
            if (Placeholders[i] != n.Placeholders[i])
                return Placeholders[i].CompareTo(n.Placeholders[i]);
        }

        return 0;
    }

    public int CompareTo(long n)
    {
        return GetValue().CompareTo(n);
    }

    public long GetValue()
    {
        long total = 0;
        try
        {
            checked
            {
                for (int i = 0; i < NumberOfPlaces; i++)
                {
                    total += (long)(Placeholders[i] * Mathf.Pow(1000, i));
                }
                return total;
            }
        }
        catch (OverflowException ex)
        {
            return long.MaxValue;
        }
        return total;
    }

    public LargeNumber Clone()
    {
        LargeNumber c = new LargeNumber();
        c.NumberOfPlaces = NumberOfPlaces;
        for (int i = 0; i < c.NumberOfPlaces; i++)
            c.Placeholders[i] = Placeholders[i];
        return c;
    }

    public static bool operator >(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) > 0;
    }

    public static bool operator <(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) < 0;
    }

    public static bool operator >=(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) >= 0;
    }

    public static bool operator <=(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) <= 0;
    }

    public static bool operator ==(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) == 0;
    }

    public static bool operator !=(LargeNumber us, LargeNumber them)
    {
        return us.CompareTo(them) != 0;
    }

    public static bool operator >(LargeNumber us, long them)
    {
        return us.CompareTo(them) > 0;
    }

    public static bool operator <(LargeNumber us, long them)
    {
        return us.CompareTo(them) < 0;
    }

    public static bool operator >=(LargeNumber us, long them)
    {
        return us.CompareTo(them) >= 0;
    }

    public static bool operator <=(LargeNumber us, long them)
    {
        return us.CompareTo(them) <= 0;
    }

    public static bool operator ==(LargeNumber us, long them)
    {
        return us.CompareTo(them) == 0;
    }

    public static bool operator !=(LargeNumber us, long them)
    {
        return us.CompareTo(them) != 0;
    }

    public bool Equals(LargeNumber n)
    {
        return CompareTo(n) == 0;
    }

    public bool Equals(long n)
    {
        return CompareTo(n) == 0;
    }

    public override bool Equals(object n)
    {
        LargeNumber them = n as LargeNumber;
        if (them != null) return Equals(them);

        long? val = n as long?;
        if (val.HasValue) return Equals(val.Value);

        return false;
    }

    protected void Carry()
    {
        byte x = 0;
        byte lastPlaceWithNumbers = 0;
        while (x < NumberOfPlaces)
        {
            if (Placeholders[x] < 0)
            {
                var borrow = Math.Abs((long)Math.Floor(Placeholders[x] / 1000.0f));
                Placeholders[x + 1] -= borrow;
                Placeholders[x] += (borrow * 1000);
            }

            if (Placeholders[x] > 0)
                lastPlaceWithNumbers = x;

            if (Placeholders[x] > 1000)
            {
                Placeholders[x + 1] += (long)Math.Floor(Placeholders[x] / 1000.0f);
                Placeholders[x] = (long)(Placeholders[x] % 1000);
                if (x == NumberOfPlaces - 1)
                    NumberOfPlaces += 1;
            }

            x++;
        }

        NumberOfPlaces = (byte)(lastPlaceWithNumbers + 1);
        if (NumberOfPlaces == 0)
            Reset();
    }

    public void Add(long value)
    {
        Placeholders[0] += value;
        Carry();
    }

    public void Add(LargeNumber number)
    {
        if (number.NumberOfPlaces > NumberOfPlaces)
            NumberOfPlaces = number.NumberOfPlaces;

        for (int i = 0; i < number.NumberOfPlaces; i++)
        {
            Placeholders[i] += number.Placeholders[i];
        }

        Carry();
    }
}
