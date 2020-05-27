/* Stammt noch von damals aus dem KDF woran ich ja auch mitgearbeitet habe
 * @author 3lit
 */
using System.Text;

namespace KDF.Networks.Protocol
{
  internal class Decompress
  {
    private object[] b = new object[2];
    private long c = 0;
    private long d = 0;
    private int g = 0;
    private int h = 0;
    private object aObject;
    private bool e;
    private byte[] f;

    internal Decompress(string paramString)
    {
      this.a(this.b, paramString);
    }

    private void a(object[] paramArrayOfObject, string paramString)
    {
      int index = 0;
      int length1 = paramString.Length;
      int paramInt1_1 = 1;
      int num1 = -33;
      int length2;
      for (; index < length1; index += length2 + 1)
      {
        int num2 = (int) paramString[index];
        int paramInt2;
        if (num2 == (int) byte.MaxValue)
        {
          length2 = (int) paramString[index + 1] + 1;
          paramInt2 = (int) paramString[index + 2];
          index += 2;
        }
        else
        {
          length2 = num2 / 21 + 1;
          paramInt2 = num2 % 21;
        }
        if ((paramInt1_1 & 1) == 0)
        {
          ++paramInt1_1;
          for (; num1 < paramInt2; ++num1)
            paramInt1_1 <<= 1;
        }
        else
        {
          do
          {
            paramInt1_1 >>= 1;
            --num1;
          }
          while ((paramInt1_1 & 1) == 1);
          ++paramInt1_1;
          for (; num1 < paramInt2; ++num1)
            paramInt1_1 <<= 1;
        }
        int paramInt1_2 = this.a(paramInt1_1, paramInt2);
        int length3 = paramString.Length;
        string paramString1 = paramString.Substring(index + 1, length2);
        if (length2 == 3 && paramString1.Equals("\\\\\\"))
          this.aObject = (object) paramString1;
        this.a(paramArrayOfObject, paramString1, paramInt1_2, paramInt2);
      }
    }

    internal string Run(byte[] paramArrayOfByte)
    {
      if (paramArrayOfByte == null)
        return (string) null;
      StringBuilder stringBuilder = new StringBuilder(paramArrayOfByte.Length * 100 / 60);
      this.f = paramArrayOfByte;
      this.g = 0;
      this.h = 0;
      this.e = false;
      object[] b = this.b;
      while (!this.e)
      {
        b = (object[]) b[this.a()];
        if (b[0] == null)
        {
          if (b[1] == this.aObject)
          {
            int num = 0;
            for (int index = 0; index < 16; ++index)
              num += this.a() << index;
            stringBuilder.Append((char) num);
          }
          else
            stringBuilder.Append((string) b[1]);
          b = this.b;
        }
      }
      string str = stringBuilder.ToString();
      this.d += (long) str.Length;
      this.c += (long) paramArrayOfByte.Length;
      return str;
    }

    private bool a(object[] paramArrayOfObject, string paramString, int paramInt1, int paramInt2)
    {
      if (paramInt2 == 0)
      {
        paramArrayOfObject[1] = (object) paramString;
        return paramArrayOfObject[0] == null;
      }
      if (paramArrayOfObject[0] == null)
      {
        if (paramArrayOfObject[1] != null)
          return false;
        paramArrayOfObject[0] = (object) new object[2];
        paramArrayOfObject[1] = (object) new object[2];
      }
      return this.a((object[]) paramArrayOfObject[paramInt1 & 1], paramString, paramInt1 >> 1, paramInt2 - 1);
    }

    private int a()
    {
      int num = 0;
      if (((int) this.f[this.h] & 1 << this.g) != 0)
        num = 1;
      ++this.g;
      if (this.g > 7)
      {
        this.g = 0;
        ++this.h;
        this.e = this.h == this.f.Length;
      }
      return num;
    }

    private int a(int paramInt1, int paramInt2)
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
  }
}
