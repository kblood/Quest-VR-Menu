using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

public class AndroidNativeFunctionsEditor : EditorWindow {
	
	public bool isGeneratePublicKey;
	public string base64EncodedKey;
	public string publicKey;

	[MenuItem("Window/Android Native Functions")]
	public static void WindowOpen(){
		EditorWindow.GetWindow<AndroidNativeFunctionsEditor>(true, "Android Native Functions", true);
	}
	
	void OnGUI(){
		isGeneratePublicKey = EditorGUILayout.Foldout(isGeneratePublicKey,"Generate Public Key");
		if(isGeneratePublicKey){
			EditorGUILayout.LabelField("Base64 Encoded Key");
			base64EncodedKey = EditorGUILayout.TextField(base64EncodedKey);
			EditorGUILayout.LabelField("Public Key");
			publicKey = EditorGUILayout.TextArea(publicKey);
			if(GUILayout.Button("Generate Public Key")){
				if(string.IsNullOrEmpty(base64EncodedKey))
					return;
				publicKey = PEMKeyLoader.CryptoServiceProviderFromPublicKeyInfo(Convert.FromBase64String(base64EncodedKey)).ToXmlString(false);
			}
		}
		
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		
		if(GUILayout.Button("Documentation",GUILayout.MaxWidth(100),GUILayout.MinHeight(20))){
			Application.OpenURL("https://docs.google.com/document/d/1CJzaeXxBSbbVvzWRXmxOLpcz79yCvDlYIZPfoAshap8/pub?embedded=true");
		}
		
		if(GUILayout.Button("Demo APK",GUILayout.MaxWidth(100),GUILayout.MinHeight(20))){
			Application.OpenURL("https://drive.google.com/open?id=0B-frjQ_v6A5tWDNVc0UyTTdiZ3M");
		}
		
		EditorGUILayout.EndHorizontal();
	}
	
	public class PEMKeyLoader{

        static byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };

        private static bool CompareBytearrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }

	    public static RSACryptoServiceProvider CryptoServiceProviderFromPublicKeyInfo(byte[] x509key)
	    {
	        byte[] seq = new byte[15];
	
	        if (x509key == null || x509key.Length == 0)
	            return null;
	        
	
	        // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
	        MemoryStream mem = new MemoryStream(x509key);
	        BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
	        byte bt = 0;
	        ushort twobytes = 0;
	
	        try
	        {
	            twobytes = binr.ReadUInt16();
	            if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
	                binr.ReadByte();	//advance 1 byte
	            else if (twobytes == 0x8230)
	                binr.ReadInt16();	//advance 2 bytes
	            else
	                return null;
	
	            seq = binr.ReadBytes(15);		//read the Sequence OID
	            if (!CompareBytearrays(seq, SeqOID))	//make sure Sequence for OID is correct
	                return null;
	
	            twobytes = binr.ReadUInt16();
	            if (twobytes == 0x8103)	//data read as little endian order (actual data order for Bit String is 03 81)
	                binr.ReadByte();	//advance 1 byte
	            else if (twobytes == 0x8203)
	                binr.ReadInt16();	//advance 2 bytes
	            else
	                return null;
	
	            bt = binr.ReadByte();
	            if (bt != 0x00)		//expect null byte next
	                return null;
	
	            twobytes = binr.ReadUInt16();
	            if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
	                binr.ReadByte();	//advance 1 byte
	            else if (twobytes == 0x8230)
	                binr.ReadInt16();	//advance 2 bytes
	            else
	                return null;
	
	            twobytes = binr.ReadUInt16();
	            byte lowbyte = 0x00;
	            byte highbyte = 0x00;
	
	            if (twobytes == 0x8102)	//data read as little endian order (actual data order for Integer is 02 81)
	                lowbyte = binr.ReadByte();	// read next bytes which is bytes in modulus
	            else if (twobytes == 0x8202)
	            {
	                highbyte = binr.ReadByte();	//advance 2 bytes
	                lowbyte = binr.ReadByte();
	            }
	            else
	                return null;
	            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
	            int modsize = BitConverter.ToInt32(modint, 0);
	
	            int firstbyte = binr.PeekChar();
	            if (firstbyte == 0x00)
	            {	//if first byte (highest order) of modulus is zero, don't include it
	                binr.ReadByte();	//skip this null byte
	                modsize -= 1;	//reduce modulus buffer size by 1
	            }
	
	            byte[] modulus = binr.ReadBytes(modsize);	//read the modulus bytes
	
	            if (binr.ReadByte() != 0x02)			//expect an Integer for the exponent data
	                return null;
	            int expbytes = (int)binr.ReadByte();		// should only need one byte for actual exponent data (for all useful values)
	            byte[] exponent = binr.ReadBytes(expbytes);
	
	
	            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
	            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
	            RSAParameters RSAKeyInfo = new RSAParameters();
	            RSAKeyInfo.Modulus = modulus;
	            RSAKeyInfo.Exponent = exponent;
	            RSA.ImportParameters(RSAKeyInfo);
	
	            return RSA;
	        }
	        finally
	        {
	            binr.Close();
	        }
	    }
	}
}
