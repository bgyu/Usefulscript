If you use ssh to connect to remove session,
when use key-bindings Ctrl-C, instead of ending current process,
it kills the ssh session.

This issue is very obvious when you use emacs (no X) in ssh session.
Emacs has many key bindings related to Ctrl-C.
When you prcess Ctrl-x Ctrl-c in emacs over ssh connection,
instead of exiting emacs, it kills the ssh connection.

For example, when you use `podman machine ssh` login to the podman machine under Windows.
If you press Ctrl-c in the terminal, you expect to kill current process,
but instead it exits ssh connection.

To solve this problem, you can use `ssh -t` to login to podman machine,
instead of using `podman machine ssh`.

First, you can use `podman machine inspect` to get ssh connection information,
like user, port, Identityfile, then in C:\Users\<Username>\.ssh\config file,
add a new entry:
```
Host podman-machine-default
  HostName 127.0.0.1
  User user
  Port 56921
  IdentityFile C:\Users\<Username>\.ssh\podman-machine-default
```
Now you can login via `ssh -t podman-machine-default`.
