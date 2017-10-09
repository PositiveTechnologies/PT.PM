class Test extends SecurityManager implements X509TrustManager
{
	public static String passwordQuestion = "What is your favorite color";
	private final Logger logger = Logger.getLogger(MyClass.class);

	// Default password: "asdf"
	public void bar() {
		String hardcodedPasswordValueNull = null;
		this.passwordValue_hardcoded = "P@$$w0rd";
		DEFAULT_PASSWORD = "7c222fb2927d828af22f592134e8932480637c0d";
		
		instance = javax.crypto.Cipher.getInstance("RSA/NONE/NoPadding");
		instance = javax.crypto.Cipher.getInstance("DES");
		
		cookie.setPath("/");
		
		cookie2.setDomain(".example.com");
		
		Random rand = new Random();
			
		(new foo()).setSeed(12345);
		
		MessageDigest.getInstance("MD5");
		MessageDigest.getInstance("SHA-1");
		
		int res = getContext().checkCallingOrSelfPermission(permission);
		res = getContext().checkCallingOrSelfUriPermission(permission);
		
		sslf.setHostnameVerifier(SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER);
		setHostnameVerifier(new AllowAllHostnameVerifier());
		
		String address = "http://www.example.com/fetchdata.php";
		
		result = new SAXReader().read(in);
		
		new XMLUtil().parse(rowObj);
		new XMLUtil().parse(modalParams);
		new XMLUtil().parse(this.params.getStringValue("income"));
		new XMLUtil().parse(arg1.toString());
		
		context.sendStickyBroadcast(intent);
		context.sendStickyBroadcastAsUser(intent, new UserHandle());
		
		SSLSocketFactory factory = new SSLSocketFactory();
		factory.getInsecure(12, null);
		factory.getInsecure(23);
		
		Encryptor instance = ESAPI.encryptor();
		String hash1 = instance.hash(input, "2!@$(5#@532@%#$253l5#@$");
		
		context.sendBroadcast(intent);
		context.sendBroadcast(intent, "xper.permission.NO_SUCH_PERMISSION");
		
		context.registerReceiver(broadcastReceiver, intentFilter);
		context.registerReceiver(broadcastReceiver, intentFilter, "xper.permission.NO_SUCH_PERMISSION", null);

		try {
		}
		catch (Throwable t) {
		}
		catch (NullPointerException e) {
			throw e;
		}
	}

	public void CookieNotSentOverSSLNotExistsSimple() {
		Cookie emailCookieNotExistsSimple = new Cookie("emailCookieNotExistsSimple", email);
		emailCookieNotExistsSimple.setSecure(true);
		response.addCookie(emailCookieNotExistsSimple);
	}
	
	public void CookieNotSentOverSSLNotExistsComplex() {
		String address = "http://www.example.com/fetchdata.php";
		Cookie emailCookieNotExistsComplex = new Cookie("emailCookieNotExistsComplex", email);
		context.sendStickyBroadcast(intent);
		context.sendStickyBroadcastAsUser(intent, new UserHandle());
		emailCookieNotExistsComplex.setSecure(true);
		String hash1 = instance.hash(input, "2!@$(5#@532@%#$253l5#@$");
		response.addCookie(emailCookieNotExistsComplex);
	}
	
	public void CookieNotSentOverSSLExistsSimple() {
		Cookie emailCookieExistsSimple = new Cookie("emailCookieExistsSimple", email);
		response.addCookie(emailCookieExistsSimple);
	}
	
	public void CookieNotSentOverSSLExistsComplex() {
		String address = "http://www.example.com/fetchdata.php";
		Cookie emailCookieExistsComplex = new Cookie("emailCookieExistsComplex", email);
		context.sendStickyBroadcast(intent);
		context.sendStickyBroadcastAsUser(intent, new UserHandle());
		String hash1 = instance.hash(input, "2!@$(5#@532@%#$253l5#@$");
		response.addCookie(emailCookieExistsComplex);
	}
	
	public void CookieNotSentOverSSLExistsAnotherVarName() {
		Cookie emailCookieExistsAnotherVarName = new Cookie("emailCookieExistsAnotherVarName", email);
		cookie1.setSecure(true);
		response.addCookie(emailCookieExistsAnotherVarName);
	}

	public void CookieNotSentOverSSLExistsTwoPatterns() {
		Cookie emailCookieExistsTwoPatterns = new Cookie("emailCookieExistsTwoPatterns", email);
		emailCookieExistsTwoPatterns.setSecure(true);
		response.addCookie(emailCookieExistsTwoPatterns);

		Cookie emailCookieExistsTwoPatterns1 = new Cookie("emailCookieExistsTwoPatterns1", email);
		response.addCookie(emailCookieExistsTwoPatterns1);
	}

	public Object clone() throws CloneNotSupportedException {
		return this;
	}

	@Override
	public void checkClientTrusted(final X509Certificate[] array, final String s)
		throws CertificateException {}
}