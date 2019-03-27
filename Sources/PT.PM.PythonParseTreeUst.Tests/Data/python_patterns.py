x = pbkdf2_hmac('md5', 1, 2, 3)

if x == 'not empty string':
    print('hardcoded comparison')

x = AES.new(key, 1, someVar=5, IV='hardcoded init vector')

sys.stderr.write('log info')

SECURE_BROWSER_XSS_FILTER = False

cfg.StrOpt(option1, option2, default = 'http')

x.exportKey(format='PEM')