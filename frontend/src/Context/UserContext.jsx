import React, {createContext, useState, useEffect} from 'react';
import { serverUri } from '../Context/ShopContext'

export const UserContext = createContext(null);
export const logout = async () => {
    const token = localStorage.getItem('auth-token');
    await fetch(serverUri + '/api/auth/Logout', {
        method: "POST",
        headers: {
            'Authorization': 'Bearer ' + btoa(localStorage.getItem('auth-token') + ':' + localStorage.getItem('refresh-token')),
        },
        body: JSON.stringify({accessToken: token}),
    });
    
    localStorage.removeItem('auth-token');
    localStorage.removeItem('refresh-token');
    sessionStorage.removeItem('Email');
    sessionStorage.removeItem('Fname');
    sessionStorage.removeItem('Lname');
    sessionStorage.removeItem('Id');
    console.log('User logged out');
    window.location.reload();
}
export const checkLogin = async () => {
    const authToken = localStorage.getItem('auth-token');
    const refreshToken = localStorage.getItem('refresh-token')
    if (!authToken || !refreshToken) {
        console.log('Incomplete login tokens found');
        localStorage.removeItem('auth-token');
        localStorage.removeItem('refresh-token');
        return false;
    }

    const response = await fetch(serverUri + '/api/auth/ValidateCustomer', {
        method: "GET",
        headers: {
            'Authorization': 'Bearer ' + btoa(localStorage.getItem('auth-token') + ':' + localStorage.getItem('refresh-token')),
        },
    });
    if (response.ok) {
        const userSession = await response.json();
        sessionStorage.setItem('Email', userSession.email);
        sessionStorage.setItem('Fname', userSession.fname);
        sessionStorage.setItem('Lname', userSession.lname);
        sessionStorage.setItem('Id', userSession.id);

        console.log('User session restored', userSession);
        return true
    } else {
        console.error('Failed to restore user session');
        return false
    }
}
export const UserContextProvider = props => {
    const [isLoggedIn, setIsLoggedIn] = useState(false);
    
    useEffect(() => {
        const checkUserLogin = async () => {
            const loggedIn = await checkLogin();
            setIsLoggedIn(loggedIn);
        };

        checkUserLogin();
    }, []);
    
    const contextValue = {checkLogin, logout, isLoggedIn, setIsLoggedIn};
    return (
        <UserContext.Provider value={contextValue}>
            {props.children}
        </UserContext.Provider>
    );
};

export default UserContextProvider;