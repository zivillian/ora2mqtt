using System.Formats.Asn1;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace libgwmapi
{
    public class CertificateHandler
    {
        public X509Certificate2 Certificate => new(Properties.Resources.cert);

        public X509Certificate2 CertificateWithPrivateKey
        {
            get
            {
                var cert = Certificate;
                var rsaParameters = RSAParameters;
                var rsa = RSA.Create(rsaParameters);
                var withKey = cert.CopyWithPrivateKey(rsa);
                return withKey;
            }
        }

        public X509Certificate2Collection Chain
        {
            get
            {
                var chain = Properties.Resources.chain;
                var collection = new X509Certificate2Collection();
                collection.ImportFromPem(Encoding.ASCII.GetString(chain));
                return collection;
            }
        }

        public RSAParameters RSAParameters
        {
            get
            {
                var base64 = Encoding.ASCII.GetString(Properties.Resources.key);
                var key = Convert.FromBase64String(base64);
                AsnDecoder.ReadSequence(key, AsnEncodingRules.DER, out var contentOffset, out var contentLength, out var bytesConsumed, Asn1Tag.Sequence);
                var data = key.AsSpan(contentOffset, contentLength);
                AsnDecoder.ReadInteger(data, AsnEncodingRules.DER, out bytesConsumed, Asn1Tag.Integer);
                data = data.Slice(bytesConsumed);
                var n = AsnDecoder.ReadInteger(data, AsnEncodingRules.DER, out bytesConsumed, Asn1Tag.Integer);
                data = data.Slice(bytesConsumed);
                AsnDecoder.ReadInteger(data, AsnEncodingRules.DER, out bytesConsumed, Asn1Tag.Integer);
                data = data.Slice(bytesConsumed);
                var transformedD = AsnDecoder.ReadInteger(data, AsnEncodingRules.DER, out bytesConsumed, Asn1Tag.Integer);
                var d = Untransform(transformedD);

                var e = new BigInteger(Certificate.PublicKey.GetRSAPublicKey().ExportParameters(false).Exponent);
                var (p, q) = RecoverPQ(n, e, d);

                var dp = d % (p - 1);
                var dq = d % (q - 1);

                var qInv = ModInverse(q, p);

                return new RSAParameters
                {
                    Modulus = n.ToByteArray(true, true),
                    Exponent = e.ToByteArray(true, true),
                    D = d.ToByteArray(true, true),
                    P = p.ToByteArray(true, true),
                    Q = q.ToByteArray(true, true),
                    DP = dp.ToByteArray(true, true),
                    DQ = dq.ToByteArray(true, true),
                    InverseQ = qInv.ToByteArray(true, true),
                };
            }
        }

        private BigInteger Untransform(BigInteger number)
        {
            var bits = number.GetBitLength();
            var fiveBitNumberCount = bits / 5;
            if (bits % 5 != 0)
            {
                fiveBitNumberCount++;
            }

            var fiveBitNumbers = new byte[fiveBitNumberCount];
            for (int i = 1; i <= fiveBitNumberCount; i++)
            {
                fiveBitNumbers[^i] = (byte)(number & 0x1f);
                number >>= 5;
            }
            number = fiveBitNumbers[0];
            for (int i = 1; i < fiveBitNumberCount; i++)
            {
                number <<= 5;
                number |= (fiveBitNumbers[i] & 0xf8U) + (fiveBitNumbers[i] + 3 & 7);
            }

            return number;
        }

        //https://stackoverflow.com/a/32436331
        private (BigInteger p, BigInteger q) RecoverPQ(BigInteger n, BigInteger e, BigInteger d)
        {
            int nBitCount = (int)(BigInteger.Log(n, 2) + 1);

            // Step 1: Let k = de – 1. If k is odd, then go to Step 4
            BigInteger k = d * e - 1;
            if (k.IsEven)
            {
                // Step 2 (express k as (2^t)r, where r is the largest odd integer
                // dividing k and t >= 1)
                BigInteger r = k;
                BigInteger t = 0;

                do
                {
                    r = r / 2;
                    t = t + 1;
                } while (r.IsEven);

                // Step 3
                bool success = false;
                BigInteger y = 0;

                for (int i = 1; i <= 100; i++)
                {

                    // 3a
                    BigInteger g;
                    do
                    {
                        byte[] randomBytes = new byte[nBitCount / 8 + 1]; // +1 to force a positive number
                        RandomNumberGenerator.Fill(randomBytes);
                        randomBytes[randomBytes.Length - 1] = 0;
                        g = new BigInteger(randomBytes);
                    } while (g >= n);

                    // 3b
                    y = BigInteger.ModPow(g, r, n);

                    // 3c
                    if (y == 1 || y == n - 1)
                    {
                        // 3g
                        continue;
                    }

                    // 3d
                    BigInteger x;
                    for (BigInteger j = 1; j < t; j = j + 1)
                    {
                        // 3d1
                        x = BigInteger.ModPow(y, 2, n);

                        // 3d2
                        if (x == 1)
                        {
                            success = true;
                            break;
                        }

                        // 3d3
                        if (x == n - 1)
                        {
                            // 3g
                            continue;
                        }

                        // 3d4
                        y = x;
                    }

                    // 3e
                    x = BigInteger.ModPow(y, 2, n);
                    if (x == 1)
                    {

                        success = true;
                        break;

                    }

                    // 3g
                    // (loop again)
                }

                if (success)
                {
                    // Step 5
                    var p = BigInteger.GreatestCommonDivisor((y - 1), n);
                    var q = n / p;
                    return (p, q);
                }
            }
            throw new Exception("Cannot compute P and Q");
        }

        //https://stackoverflow.com/a/35250572
        private static BigInteger ModInverse(BigInteger a, BigInteger n)
        {
            BigInteger t = 0, nt = 1, r = n, nr = a;

            if (n < 0)
            {
                n = -n;
            }

            if (a < 0)
            {
                a = n - (-a % n);
            }

            while (nr != 0)
            {
                var quot = r / nr;

                var tmp = nt; nt = t - quot * nt; t = tmp;
                tmp = nr; nr = r - quot * nr; r = tmp;
            }

            if (r > 1) throw new ArgumentException(nameof(a) + " is not convertible.");
            if (t < 0) t = t + n;
            return t;
        }
    }
}