# To enabe core dump for RHEL (8.9)

* /etc/security/limits.d/10-coredump.conf
```
root soft core unlimited
```

replace `root` with your user name or use `*` to enable core dumps for all users.
You need to logout to take effect.
Can use `ulimit -c` to check if it's enabled: 'unlimited' means enabled.

* /etc/sysctl.d/99-sysctl.conf
  ```
  kernel.core_pattern=/tmp/crash/core_%e_%p_%t
  ```
Query the the setting: `sudo sysctl kernel.core_pattern`

* make sure the crash dump folder exists
```
mkdir -p /tmp/crash
chmod 755 /tmp/crash
```

* Testing core dump
```
sleep 1000&
kill -SIGSEGV <pid_of_sleep>
ls /tmp/crash  # should have a core_sleep_<pid>_<timestamp> file
```

  If you want to use `coredumpctl` to manage core dump, then don't set it. Just use default value.
  ref: https://docs.oracle.com/en/operating-systems/oracle-linux/9/monitoring/working_with_core_dumps.html#topic_ygw_fp3_fzb
