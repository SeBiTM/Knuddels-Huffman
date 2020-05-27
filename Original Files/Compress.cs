/* Stammt noch von damals aus dem KDF woran ich ja auch mitgearbeitet habe
 * @author 3lit
 */
using System;
using System.Collections;

namespace KDF.Networks.Protocol
{
  internal class Compress
  {
    private Hashtable cHashTable = new Hashtable(1, 1f);
    private long d = 0;
    private long e = 0;
    private byte[] f = new byte[(int) ushort.MaxValue];
    private int g = 0;
    private int h = 0;
    private int aInt;
    private int bInt;
    private char[] i;
    private short[] j;

    internal Compress(string paramString)
    {
      this.a();
      this.a(paramString);
    }

        internal byte[] Run(string paramString, int paramInt)
        {
            byte[] numArray1 = (byte[])null;
            if (paramInt > 0)
            {
                numArray1 = this.f;
                this.f = new byte[paramInt];
            }
            this.g = 0;
            this.h = 0;
            int length = paramString.Length;
            this.e += (long)length;
            int index = 0;
            while (index < length)
            {
                object obj = (object)null;
                int num1 = index + 1;
                Hashtable cHashTable = this.cHashTable;
                for (; index < length; ++index)
                {
                    int paramInt1 = (int)paramString[index];
                    int num2 = (int)(cHashTable[(object)this.b(paramInt1)] == null ? (object)0 : cHashTable[(object)this.b(paramInt1)]);
                    if (num2 != 0)
                    {
                        num1 = index + 1;
                        obj = (object)num2;
                    }
                    cHashTable = (Hashtable)cHashTable[(object)this.c(paramInt1)];
                    if (cHashTable == null)
                        break;
                }
                index = num1;
                if (obj == null)
                {
                    this.a(this.aInt);
                    this.a(268435456 + (int)paramString[index - 1]);
                }
                else
                    this.a((int)obj);
            }

            int h = this.h;
            if (this.g != 0)
            {
                while (h == this.h)
                    this.a(this.bInt);
            }

            byte[] numArray2 = new byte[this.h];
            Array.Copy((Array)this.f, 0, (Array)numArray2, 0, this.h);
            this.d += (long)this.h;
            if (paramInt > 0)
                this.f = numArray1;
            return numArray2;
        }

    private void a(int paramInt)
    {
      int num1 = paramInt >> 24;
      int num2 = paramInt - (num1 << 24);
      if (this.g != 0 && num1 > 0)
      {
        num1 += this.g - 8;
        int num3 = num2 << this.g;
        int index = this.h++;
        byte[] f = this.f;
        f[index] = (byte) ((uint) f[index] + (uint) (byte) num3);
        num2 = num3 >> 8;
        this.g = 0;
      }
      for (; num1 > 0; num1 -= 8)
      {
        this.f[this.h++] = (byte) num2;
        num2 >>= 8;
      }
      if (num1 >= 0)
        return;
      --this.h;
      this.g = num1 + 8;
    }

    private void a(string pTree)
    {
      int index = 0;
      int length1 = textToCompress.Length;
      int paramInt1 = 1;
      int num1 = -33;
      int length2;
      for (; index < length1; index += length2 + 1)
      {
        int num2 = (int) textToCompress[index];
        int paramInt2;
        if (num2 == (int) byte.MaxValue)
        {
          length2 = (int) textToCompress[index + 1] + 1;
          paramInt2 = (int) textToCompress[index + 2];
          index += 2;
        }
        else
        {
          length2 = num2 / 21 + 1;
          paramInt2 = num2 % 21;
        }
        if ((paramInt1 & 1) == 0)
        {
          ++paramInt1;
          for (; num1 < paramInt2; ++num1)
            paramInt1 <<= 1;
        }
        else
        {
          do
          {
            paramInt1 >>= 1;
            --num1;
          }
          while ((paramInt1 & 1) == 1);
          ++paramInt1;
          for (; num1 < paramInt2; ++num1)
            paramInt1 <<= 1;
        }
        int num3 = this.ab(paramInt1, paramInt2) + (paramInt2 << 24);
        string paramString = textToCompress.Substring(index + 1, length2);
        if (this.bInt == 0 && paramInt2 > 8)
          this.bInt = this.ab(paramInt1 >> paramInt2 - 8, 8) + 134217728;
        if (length2 == 3 && paramString.Equals("\\\\\\"))
          this.aInt = num3;
        else
          this.a(this.cHashTable, paramString, 0, (object) num3);
      }
    }

    private void a()
    {
      this.i = new char[256];
      this.j = new short[256];
      for (int index = 0; index < this.i.Length; ++index)
      {
        this.i[index] = (char) index;
        this.j[index] = (short) index;
      }
    }

    private void a(Hashtable paramHashtable, string paramString, int paramInt, object paramObject)
    {
      int paramInt1 = (int) paramString[paramInt];
      if (paramInt + 1 >= paramString.Length)
      {
        if (paramHashtable[(object) this.b(paramInt1)] != null)
          throw new Exception("Error: " + paramString);
        paramHashtable[(object) this.b(paramInt1)] = paramObject;
      }
      else
      {
        Hashtable paramHashtable1 = (Hashtable) paramHashtable[(object) this.c(paramInt1)];
        if (paramHashtable1 == null)
        {
          paramHashtable1 = new Hashtable(1, 1f);
          paramHashtable[(object) this.c(paramInt1)] = (object) paramHashtable1;
        }
        this.a(paramHashtable1, paramString, paramInt + 1, paramObject);
      }
    }

    private int ab(int paramInt1, int paramInt2)
    {
      int num1 = 0;
      int num2 = 1;
      int num3 = 1 << paramInt2 - 1;
      for (; paramInt2 > 0; --paramInt2)
      {
        if ((paramInt1 & num2) != 0)
          num1 += num3;
        num2 <<= 1;
        num3 >>= 1;
      }
      return num1;
    }

    private char b(int paramInt)
    {
      paramInt &= (int) ushort.MaxValue;
      return paramInt < 256 ? this.i[paramInt] : (char) paramInt;
    }

    private short c(int paramInt)
    {
      paramInt &= (int) ushort.MaxValue;
      return paramInt < 256 ? this.j[paramInt] : (short) paramInt;
    }
  }
}
