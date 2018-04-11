package com.shoestore.service;

import com.shoestore.model.User;

public interface UserService {
	public User findUserByEmail(String email);
	public void saveUser(User user);
}
