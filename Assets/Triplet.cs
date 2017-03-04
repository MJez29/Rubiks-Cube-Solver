using System;

public class Pair<T1, T2> : IEquatable<Object>
{
    public T1 Item1
    {
        get;
        set;
    }

    public T2 Item2
    {
        get;
        set;
    }

    public Pair(T1 Item1, T2 Item2)
    {
        this.Item1 = Item1;
        this.Item2 = Item2;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || (obj as Pair<T1, T2>) == null) //if the object is null or the cast fails
            return false;
        else
        {
            Pair<T1, T2> pair = (Pair<T1, T2>)obj;
            return Item1.Equals(pair.Item1) && Item2.Equals(pair.Item2);
        }
    }

    public override int GetHashCode()
    {
        return Item1.GetHashCode() ^ Item2.GetHashCode();
    }
    /*
    public static bool operator ==(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
    {
        return pair1.Equals(pair2);
    }

    public static bool operator !=(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
    {
        return !pair1.Equals(pair2);
    }
    */
    public override string ToString()
    {
        return "Pair<" + typeof(T1) + ", " + typeof(T2) + ">(" + Item1.ToString() + ", " + Item2.ToString() + ")";
    }
}

public class Triplet<T1, T2, T3> : IEquatable<Object>
{
    public T1 Item1
    {
        get;
        set;
    }

    public T2 Item2
    {
        get;
        set;
    }

    public T3 Item3
    {
        get;
        set;
    }

    public Triplet(T1 Item1, T2 Item2, T3 Item3)
    {
        this.Item1 = Item1;
        this.Item2 = Item2;
        this.Item3 = Item3;
    }

    public bool Contains(object obj)
    {
        if (obj != null)
        {
            if (obj is T1 && Item1.Equals((T1)obj))
                return true;
            if (obj is T2 && Item2.Equals((T2)obj))
                return true;
            if (obj is T3 && Item3.Equals((T3)obj))
                return true;
        }
        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || (obj as Triplet<T1, T2, T3>) == null) //if the object is null or the cast fails
            return false;
        else
        {
            Triplet<T1, T2, T3> tuple = (Triplet<T1, T2, T3>)obj;
            return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2) && Item3.Equals(tuple.Item3);
        }
    }

    public override int GetHashCode()
    {
        return Item1.GetHashCode() ^ Item2.GetHashCode() ^ Item3.GetHashCode();
    }
    /*
    public static bool operator ==(Triplet<T1, T2, T3> tuple1, Triplet<T1, T2, T3> tuple2)
    {
        return tuple1.Equals(tuple2);
    }

    public static bool operator !=(Triplet<T1, T2, T3> tuple1, Triplet<T1, T2, T3> tuple2)
    {
        try {
            return !tuple1.Equals(tuple2);
        } catch (NullReferenceException e) {

        }
        
    }*/

    public override string ToString()
    {
        return "Triplet<" + typeof(T1) + ", " + typeof(T2) + ", " + typeof(T3) + ">(" + Item1.ToString() + ", " + Item2.ToString() + ", " + Item3.ToString() + ")";
    }
}