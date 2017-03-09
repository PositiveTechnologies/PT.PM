<?php
    // Password in comment. Default password: "asdf"

    // Hardcoded Password
    
    $storedPassword = NULL;

    if (($temp = getPassword()) != NULL) {
      $storedPassword = $temp;
    }

    if(strcmp($storedPassword,$userPassword) == 0) {
    }
    
    $connection = mysql_connect($host, 'scott', '');
    $link = mysql_connect($url, 'scott', 'tiger');
    if (!$link) {
        die('Could not connect: ' . mysql_error());
    }
    
    // Insecure Randomness
    
    function genReceiptURL($baseURL) {
        $randNum = rand();
        $receiptURL = $baseURL . $randNum . ".html";
        return $receiptURL;
    }
    
    // Insecure Transport
    
    $client = new Zend_Http_Client('http://www.example.com/fetchdata.php');
    $client->request(Zend_Http_Client::POST);
    
    // HTML5: Overly Permissive CORS Policy
    
    header('Access-Control-Allow-Origin:  *');
    
    // Weak Encryption: Inadequate RSA Padding
    
    function encrypt($input, $key) {
        $output='';
        openssl_public_encrypt($input, $output, $key, OPENSSL_NO_PADDING);
        return $output;
    }
    
    // Weak Encryption: Broken or Risky Cryptographic Algorithm
    $cipher = mcrypt_module_open(MCRYPT_DES, '', $mode, '');
    
    // Weak Cryptographic Hash
    function createSalt()
    {
        $string = md5(uniqid(rand(), true));
        return substr($string, 0, 3);
    }
    $salt = createSalt();
    $hash = sha1($salt . $hash);
    
    // CakePHP Misconfiguration: Excessive Session Timeout
    
    Configure::write('Security.level', 'low');
    
    // CakePHP Misconfiguration: Debug Information
    
    Configure::write('debug', 3);
    Configure::write('debug', 50); // Shoud not be matched.
    
    // System Information Leak
    
    echo "Server error! Printing the backtrace";
    debug_print_backtrace();
    
    // Weak Cryptographic Hash: Hardcoded Salt
    
    crypt($password, '2!@$(5#@532@%#$253l5#@$');
    
    // Key Management: Null Encryption Key
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

    setcookie("mySessionId", getSessionID(), 0, "/", "communitypages.example.com", true, true);

    setcookie("mySessionId", getSessionID(), 0, "communitypages", ".example.com", true, true);

    setcookie("emailCookie", $email, 0, "/", "www.example.com", TRUE);  //Missing 7th parameter to set HttpOnly

    setcookie("emailCookie", $email, 0, "/", "www.example.com");
    
    try {
        echo 1/0;
    } catch (Exception $e) {
    }
?>