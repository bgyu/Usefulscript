# How to use ansible to install a group of software packages in several rhel 7.9 nodes?

To use Ansible for installing a group of software packages on several RHEL 7.9 nodes, you'll need to follow these steps:

## 1. Install Ansible: Ensure that Ansible is installed on your control node (the machine where you run Ansible commands). 
You can follow the official installation guide here: https://docs.ansible.com/ansible/latest/installation_guide/intro_installation.html

## 2. Set up an inventory file: Create a file called inventory.ini in a directory of your choice. 
This file will contain the list of RHEL 7.9 nodes you want to manage. 
Replace the <user> and <host> placeholders with your actual SSH user and target node IP address or hostname.

inventory.ini
```
[rhel_nodes]
node1 ansible_host=<host> ansible_user=<user>
node2 ansible_host=<host> ansible_user=<user>
```

## 3. Create a playbook: Create a new file called *install_packages.yml* in the same directory as your inventory file. 
This file will contain the tasks to install the software packages. Replace the <package1>, <package2>, etc. 
placeholders with the actual package names you want to install.

install_packages.yml  
```yaml 
---
- name: Install software packages on RHEL 7.9 nodes
  hosts: rhel_nodes
  become: yes
  tasks:
    - name: Ensure required packages are installed
      yum:
        name:
          - <package1>
          - <package2>
          - <package3>
        state: present
```
  
## 4. Run the playbook: Use the following command to execute the playbook and install the specified software packages on the RHEL 7.9 nodes:
  
```bash
ansible-playbook -i inventory.ini install_packages.yml
```
  
This command tells Ansible to use the inventory file inventory.ini and the playbook install_packages.yml. 
Ansible will connect to the RHEL 7.9 nodes specified in the inventory file and perform the tasks defined in the playbook, which in this case, 
is installing the specified software packages.

  
  
