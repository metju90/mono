//
// BootstrapContextTest.cs - NUnit Test Cases for System.IdentityModel.Tokens.BootstrapContext
//
#if !MOBILE
using System;
using System.IO;
using System.IdentityModel.Tokens;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Tokens.net_4_5 {
	[TestFixture]
	public class BootstrapContextTest {
		// The following byte arrays are the serialized bytes as emitted on Microsoft .Net 4.5.
		private static readonly byte [] SerializedBootstrapContextByteArray = new byte [] { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x57, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2C, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x34, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20, 0x43, 0x75, 0x6C, 0x74, 0x75, 0x72, 0x65, 0x3D, 0x6E, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C, 0x20, 0x50, 0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B, 0x65, 0x79, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x3D, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00, 0x00, 0x00, 0x2C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2E, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x73, 0x2E, 0x42, 0x6F, 0x6F, 0x74, 0x73, 0x74, 0x72, 0x61, 0x70, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74, 0x02, 0x00, 0x00, 0x00, 0x01, 0x4B, 0x01, 0x54, 0x00, 0x07, 0x03, 0x02, 0x02, 0x00, 0x00, 0x00, 0x42, 0x09, 0x03, 0x00, 0x00, 0x00, 0x0F, 0x03, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x02, 0x01, 0x0B };
		private static readonly byte [] SerializedBootstrapContextString = new byte [] { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x57, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2C, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x34, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20, 0x43, 0x75, 0x6C, 0x74, 0x75, 0x72, 0x65, 0x3D, 0x6E, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C, 0x20, 0x50, 0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B, 0x65, 0x79, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x3D, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00, 0x00, 0x00, 0x2C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2E, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x73, 0x2E, 0x42, 0x6F, 0x6F, 0x74, 0x73, 0x74, 0x72, 0x61, 0x70, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74, 0x02, 0x00, 0x00, 0x00, 0x01, 0x4B, 0x01, 0x54, 0x00, 0x01, 0x03, 0x02, 0x00, 0x00, 0x00, 0x53, 0x06, 0x03, 0x00, 0x00, 0x00, 0x05, 0x74, 0x6F, 0x6B, 0x65, 0x6E, 0x0B };

		// Put in some non-ascii/latin1 characters to test the encoding scheme
		// \u018E == Latin capital letter Reversed E
		private const string user = "us\u018Er";
		// \u00BD == Vulgar Fraction one half
		// [SuppressMessage("Microsoft.Security", "CS002:SecretInNextLine", Justification="Not a secret.")]
		private const string password = "pass\u00BDword";
		private static readonly string SerializedBootstrapContextSecurityTokenString = "<UserNameSecurityToken Id=\"uuid-927c0b98-ba18-49d2-a653-306d60f85751-3\" Username=\"" + user + "\" Password=\"" + password + "\"/>";
		private static readonly byte [] SerializedBootstrapContextSecurityToken = new byte [] { 0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x57, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2C, 0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x34, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20, 0x43, 0x75, 0x6C, 0x74, 0x75, 0x72, 0x65, 0x3D, 0x6E, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C, 0x20, 0x50, 0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B, 0x65, 0x79, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x3D, 0x62, 0x37, 0x37, 0x61, 0x35, 0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39, 0x05, 0x01, 0x00, 0x00, 0x00, 0x2C, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x49, 0x64, 0x65, 0x6E, 0x74, 0x69, 0x74, 0x79, 0x4D, 0x6F, 0x64, 0x65, 0x6C, 0x2E, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x73, 0x2E, 0x42, 0x6F, 0x6F, 0x74, 0x73, 0x74, 0x72, 0x61, 0x70, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x78, 0x74, 0x02, 0x00, 0x00, 0x00, 0x01, 0x4B, 0x01, 0x54, 0x00, 0x01, 0x03, 0x02, 0x00, 0x00, 0x00, 0x54, 0x06, 0x03, 0x00, 0x00, 0x00, 0x98, 0x01, 0x50, 0x46, 0x56, 0x7A, 0x5A, 0x58, 0x4A, 0x4F, 0x59, 0x57, 0x31, 0x6C, 0x55, 0x32, 0x56, 0x6A, 0x64, 0x58, 0x4A, 0x70, 0x64, 0x48, 0x6C, 0x55, 0x62, 0x32, 0x74, 0x6C, 0x62, 0x69, 0x42, 0x4A, 0x5A, 0x44, 0x30, 0x69, 0x64, 0x58, 0x56, 0x70, 0x5A, 0x43, 0x30, 0x35, 0x4D, 0x6A, 0x64, 0x6A, 0x4D, 0x47, 0x49, 0x35, 0x4F, 0x43, 0x31, 0x69, 0x59, 0x54, 0x45, 0x34, 0x4C, 0x54, 0x51, 0x35, 0x5A, 0x44, 0x49, 0x74, 0x59, 0x54, 0x59, 0x31, 0x4D, 0x79, 0x30, 0x7A, 0x4D, 0x44, 0x5A, 0x6B, 0x4E, 0x6A, 0x42, 0x6D, 0x4F, 0x44, 0x55, 0x33, 0x4E, 0x54, 0x45, 0x74, 0x4D, 0x79, 0x49, 0x67, 0x56, 0x58, 0x4E, 0x6C, 0x63, 0x6D, 0x35, 0x68, 0x62, 0x57, 0x55, 0x39, 0x49, 0x6E, 0x56, 0x7A, 0x78, 0x6F, 0x35, 0x79, 0x49, 0x69, 0x42, 0x51, 0x59, 0x58, 0x4E, 0x7A, 0x64, 0x32, 0x39, 0x79, 0x5A, 0x44, 0x30, 0x69, 0x63, 0x47, 0x46, 0x7A, 0x63, 0x38, 0x4B, 0x39, 0x64, 0x32, 0x39, 0x79, 0x5A, 0x43, 0x49, 0x76, 0x50, 0x67, 0x3D, 0x3D, 0x0B };

		[Test]
		public void Ctor_StringToken_Works ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext ("token");

			Assert.AreEqual ("token", bootstrapContext.Token, "#1");
			Assert.IsNull (bootstrapContext.TokenBytes, "#2");
			Assert.IsNull (bootstrapContext.SecurityToken, "#3");
			Assert.IsNull (bootstrapContext.SecurityTokenHandler, "#4");
		}
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_StringToken_NullToken_Throws ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext ((string)null);
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void Serialize_StringToken_Works ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext ("token");
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream ()) {
				binaryFormatter.Serialize (s, bootstrapContext);
				s.Position = 0;
				BootstrapContext bootstrapContext2 = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext2, "#1");
				Assert.AreEqual (bootstrapContext.Token, bootstrapContext2.Token, "#2");
				Assert.AreEqual (bootstrapContext.TokenBytes, bootstrapContext2.TokenBytes, "#3");
				Assert.AreEqual (bootstrapContext.SecurityToken, bootstrapContext2.SecurityToken, "#4");
				Assert.AreEqual (bootstrapContext.SecurityTokenHandler, bootstrapContext2.SecurityTokenHandler, "#5");
			}
		}

		[Test]
		public void Deserialize_StringToken_Works ()
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream (SerializedBootstrapContextString)) {
				BootstrapContext bootstrapContext = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext, "#1");
				Assert.AreEqual ("token", bootstrapContext.Token, "#2");
				Assert.IsNull (bootstrapContext.TokenBytes, "#3");
				Assert.IsNull (bootstrapContext.SecurityToken, "#4");
				Assert.IsNull (bootstrapContext.SecurityTokenHandler, "#5");
			}
		}

		[Test]
		public void Ctor_ByteArrayToken_Works ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext (new byte [] { 0x01 });

			Assert.IsNotNull (bootstrapContext.TokenBytes, "#1");
			Assert.AreEqual (1, bootstrapContext.TokenBytes.Length, "#2");
			Assert.AreEqual (1, bootstrapContext.TokenBytes [0], "#3");
			Assert.IsNull (bootstrapContext.Token, "#4");
			Assert.IsNull (bootstrapContext.SecurityToken, "#5");
			Assert.IsNull (bootstrapContext.SecurityTokenHandler, "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_ByteArrayToken_NullToken_Throws ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext ((byte [])null);
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void Serialize_ByteArrayToken_Works ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext (new byte [] { 0x1 });
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream ()) {
				binaryFormatter.Serialize (s, bootstrapContext);
				s.Position = 0;
				BootstrapContext bootstrapContext2 = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext2, "#1");
				Assert.AreEqual (bootstrapContext.Token, bootstrapContext2.Token, "#2");
				Assert.AreEqual (bootstrapContext.TokenBytes, bootstrapContext2.TokenBytes, "#3");
				Assert.AreEqual (bootstrapContext.SecurityToken, bootstrapContext2.SecurityToken, "#4");
				Assert.AreEqual (bootstrapContext.SecurityTokenHandler, bootstrapContext2.SecurityTokenHandler, "#5");
			}
		}

		[Test]
		public void Deserialize_ByteArrayToken_Works ()
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream (SerializedBootstrapContextByteArray)) {
				BootstrapContext bootstrapContext = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext, "#1");
				Assert.IsNotNull (bootstrapContext.TokenBytes, "#2");
				Assert.AreEqual (1, bootstrapContext.TokenBytes.Length, "#3");
				Assert.AreEqual (1, bootstrapContext.TokenBytes [0], "#4");
				Assert.IsNull (bootstrapContext.Token, "#5");
				Assert.IsNull (bootstrapContext.SecurityToken, "#6");
				Assert.IsNull (bootstrapContext.SecurityTokenHandler, "#7");
			}
		}

		[Test]
		public void Ctor_SecurityToken_Works ()
		{
			var securityToken = new UserNameSecurityToken (user, password);
			var securityTokenHandler = new SimpleSecurityTokenHandler ();
			BootstrapContext bootstrapContext = new BootstrapContext (securityToken, securityTokenHandler);

			Assert.IsNotNull (bootstrapContext.SecurityToken, "#1");
			Assert.AreEqual (user, securityToken.UserName, "#2");
			Assert.AreEqual (password, securityToken.Password, "#3");
			Assert.AreEqual (securityTokenHandler, bootstrapContext.SecurityTokenHandler, "#4");

			Assert.IsNull (bootstrapContext.Token, "#5");
			Assert.IsNull (bootstrapContext.TokenBytes, "#6");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_SecurityToken_NullToken_Throws ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext (null, new SimpleSecurityTokenHandler ());
			Assert.Fail ("Should have thrown");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_SecurityToken_NullHandler_Throws ()
		{
			BootstrapContext bootstrapContext = new BootstrapContext (new UserNameSecurityToken ("user", "password"), null);
			Assert.Fail ("Should have thrown");
		}

		[Test]
		public void Serialize_SecurityTokenAndHandler_Works ()
		{
			var securityToken = new UserNameSecurityToken (user, password, "uuid-927c0b98-ba18-49d2-a653-306d60f85751-3");
			var securityTokenHandler = new SimpleSecurityTokenHandler ();
			BootstrapContext bootstrapContext = new BootstrapContext (securityToken, securityTokenHandler);

			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream ()) {
				binaryFormatter.Serialize (s, bootstrapContext);
				s.Position = 0;
				BootstrapContext bootstrapContext2 = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext2, "#1");
				// Deserialize does not restore the SecurityToken, but restores into the Token.
				Assert.IsNotNull (bootstrapContext2.Token, "#3");
				// We replace ' /' by '/' to accomodate the xml writer differences between mono and .net
				Assert.AreEqual (SerializedBootstrapContextSecurityTokenString.Replace (" /", "/"), bootstrapContext2.Token.Replace (" /", "/"), "#2");
				Assert.AreEqual (bootstrapContext.TokenBytes, bootstrapContext2.TokenBytes, "#3");
				Assert.IsNull (bootstrapContext2.SecurityToken, "#4");
				Assert.IsNull (bootstrapContext2.SecurityTokenHandler, "#5");
			}
		}

		[Test]
		public void Deserialize_SecurityTokenAndHandler_Works ()
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter ();
			using (var s = new MemoryStream (SerializedBootstrapContextSecurityToken)) {
				BootstrapContext bootstrapContext = binaryFormatter.Deserialize (s) as BootstrapContext;
				Assert.IsNotNull (bootstrapContext, "#1");
				Assert.AreEqual (SerializedBootstrapContextSecurityTokenString, bootstrapContext.Token, "#2");
				Assert.IsNull (bootstrapContext.SecurityToken, "#3");
				Assert.IsNull (bootstrapContext.SecurityTokenHandler, "#4");
				Assert.IsNull (bootstrapContext.TokenBytes, "#5");
			}
		}

		private static void DumpAsText (byte [] data)
		{
			Console.WriteLine ("{0}", Encoding.ASCII.GetString (data));
		}

		private static void Dump (byte [] data)
		{
			var sb = new StringBuilder ();
			sb.Append ("new byte[] { ");
			bool first = true;
			foreach (byte b in data) {
				if (!first)
					sb.Append (", ");
				else
					first = false;
				sb.AppendFormat ("0x{0:X2}", b);
			}
			sb.Append (" };");
			Console.WriteLine (sb.ToString ());
		}

		private class SimpleSecurityTokenHandler : SecurityTokenHandler {
			public override string [] GetTokenTypeIdentifiers ()
			{
				throw new NotImplementedException ();
			}

			public override Type TokenType {
				get { return typeof (UserNameSecurityToken); }
			}

			public override bool CanWriteToken {
				get { return true; }
			}

			public override void WriteToken (XmlWriter writer, SecurityToken token)
			{
				UserNameSecurityToken unst = token as UserNameSecurityToken;
				if (unst == null)
					throw new ArgumentException ("Token must be of type UserNameSecurityToken", "token");
				writer.WriteStartElement ("UserNameSecurityToken");
				writer.WriteAttributeString ("Id", unst.Id);
				writer.WriteAttributeString ("Username", unst.UserName);
				writer.WriteAttributeString ("Password", unst.Password);
				writer.WriteEndElement ();
			}
		}
	}
}
#endif
