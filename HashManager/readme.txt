Here's how to get started with the HashManager demo apps.

1) Invoke the make CMD shell script

cmd> make
building hashgen.exe
building pwdverify.exe
done.

2) Generate a hash and salt from a password string
cmd> hashgen password
hash: 0c134f21fce5a5e913d7632253d6633ab9ffde0bccddd307037697dbe49a0ca4
salt: 18cf8e57d1c9cd5330b992060753bfbe2e325aedd530d2388bef940d2fbbcc14

3) Verify that the password matches the hash/salt combo.
cmd> pwdverify password 0c134f21fce5a5e913d7632253d6633ab9ffde0bccddd307037697db
e49a0ca4 18cf8e57d1c9cd5330b992060753bfbe2e325aedd530d2388bef940d2fbbcc14
OK

 