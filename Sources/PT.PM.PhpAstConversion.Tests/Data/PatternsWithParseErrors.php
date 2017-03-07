<?php
    asdf 0123
    // Password in comment. Default password: "asdf"

    // Hardcoded Password
    //http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/password_management_hardcoded_password.html
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/password_management_null_password.html
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/password_management_empty_password.htm
    
    $storedPassword = NULL;

    if (($temp = getPassword()) != NULL) {
      $storedPassword = $temp;
    }
--
    if(strcmp($storedPassword,$userPassword) == 0) {
        sdd
    }
    
    $connection = mysql_connect($host, 'scott', '');
    $link = mysql_connect($url, 'scott', 'tiger');
    if (!$link) {
        die('Could not connect: ' . mysql_error());
    }
    
    // Insecure Randomness
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/insecure_randomness.html
    
    function genReceiptURL($baseURL) {
        $randNum = rand();
        $receiptURL = $baseURL . $randNum . ".html";
        return $receiptURL;
    }
    
    // Insecure Transport
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/insecure_transmission_http.html
    
    $client = new Zend_Http_Client('http://www.example.com/fetchdata.php');
    $client->request(Zend_Http_Client::POST);
    
    // HTML5: Overly Permissive CORS Policy
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/html5_overly_permissive_cors_policy.html
    
    header('Access-Control-Allow-Origin:  *');
    
    // Weak Encryption: Inadequate RSA Padding
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/weak_encryption_inadequate_rsa_padding.html
    
    function encrypt($input, $key) {
        $output='';
        openssl_public_encrypt($input, $output, $key, OPENSSL_NO_PADDING);
        return $output;
    }
    
    // Weak Encryption: Broken or Risky Cryptographic Algorithm
    $cipher = mcrypt_module_open(MCRYPT_DES, '', $mode, '');
    
    a = ''
    
    // Weak Cryptographic Hash
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/weak_cryptographic_hash.html
    function createSalt()
    {
        $string = md5(uniqid(rand(), true));
        return substr($string, 0, 3);
    }
    $salt = createSalt();
    $hash = sha1($salt . $hash);
    
    // CakePHP Misconfiguration: Excessive Session Timeout
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cakephp_misconfiguration_excessive_session_timeout.html
    
    Configure::write('Security.level', 'low');
    
    // CakePHP Misconfiguration: Debug Information
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cakephp_misconfiguration_debug_information.html
    
    Configure::write('debug', 3);
    Configure::write('debug', 50); // Shoud not be matched.
    
    // System Information Leak
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/system_information_leak.html
    
    echo "Server error! Printing the backtrace";
    debug_print_backtrace();
    
    // Weak Cryptographic Hash: Hardcoded Salt 
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/weak_cryptographic_hash_hardcoded_salt.html
    
    crypt($password, '2!@$(5#@532@%#$253l5#@$');
    
    // Key Management: Null Encryption Key
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/key_management_null_encryption_key.html
    // TODO: make searchable not only in function block.
    class Test
    {
        public function testFunc()
        {
            $encryption_key = NULL;
            echo $encryption_key;
            $filter = new Zend_Filter_Encrypt($encryption_key);
            $filter->setVector('myIV');
            $encrypted = $filter->filter('text_to_be_encrypted');
            print $encrypted;
        }
        
        public function testFunc2()
        {
            $encryption_key = 'hardcoded';
            $filter = new Zend_Filter_Encrypt($encryption_key);
        }
    }
    
    $filter = new Zend_Filter_Encrypt(null);
    $filter = new Zend_Filter_Encrypt('hardcoded');
    
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cookie_security_overly_broad_path.html
    setcookie("mySessionId", getSessionID(), 0, "/", "communitypages.example.com", true, true);

    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cookie_security_overly_broad_domain.html
    setcookie("mySessionId", getSessionID(), 0, "communitypages", ".example.com", true, true);
    
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cookie_security_httponly_not_set.html
    setcookie("emailCookie", $email, 0, "/", "www.example.com", TRUE);  //Missing 7th parameter to set HttpOnly
    
    // http://www.hpenterprisesecurity.com/vulncat/en/vulncat/php/cookie_security_cookie_not_sent_over_ssl.html
    setcookie("emailCookie", $email, 0, "/", "www.example.com");

    try {
        echo 1/0;
    } catch (Exception $e) {
    }
?>