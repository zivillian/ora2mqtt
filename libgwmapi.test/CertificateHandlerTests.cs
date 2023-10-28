using System.Numerics;
using System.Security.Cryptography;

namespace libgwmapi.test
{
    public class CertificateHandlerTests
    {
        [Fact]
        public void CanLoadCertificate()
        {
            var handler = new CertificateHandler();
            var cert = handler.Certificate;
            Assert.NotNull(cert);
            Assert.Equal("2D6AC1A47934CB3258542F52EA488E0C141D7304", cert.GetSerialNumberString());
        }

        [Fact]
        public void CanCalculateRSAParameters()
        {
            var handler = new CertificateHandler();
            var rsaParameter = handler.RSAParameters;
            Assert.NotNull(rsaParameter.Modulus);
            Assert.NotNull(rsaParameter.D);
            Assert.NotNull(rsaParameter.Exponent);
            Assert.NotNull(rsaParameter.P);
            Assert.NotNull(rsaParameter.Q);
            Assert.NotNull(rsaParameter.DP);
            Assert.NotNull(rsaParameter.DQ);
            Assert.NotNull(rsaParameter.InverseQ);
        }

        [Fact]
        public void CanCreateCertificateWithPrivateKey()
        {
            var handler = new CertificateHandler();
            var cert = handler.CertificateWithPrivateKey;
            Assert.True(cert.HasPrivateKey);
        }

        [Fact]
        public void CanLoadChain()
        {
            var handler = new CertificateHandler();
            var chain = handler.Chain;
            Assert.NotNull(chain);
            Assert.Equal(3, chain.Count);
        }
    }
}