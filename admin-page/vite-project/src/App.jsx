import React from "react";
import Navbar from "./Components/Navbar/Navbar";
import Admin from './Pages/Admin/Admin'
import { createTheme } from "@mui/material/styles";
import { cognitoUser, login, refresh, isTokenExpired } from "./auth.js";
export const serverUri = process.env.NODE_ENV === 'production'
    ? "https://nn8hvsrhhk.execute-api.ap-southeast-2.amazonaws.com"
    : "http://localhost:3000";
export const apiEndpoint = "/api/inventory"

export const getSessionTokens = async () => {
    // Get the current session
    let userSession = await new Promise((resolve, reject) => {
        cognitoUser.getSession((err, session) => {
            if (err) {
                console.error(err);
                return;
            }
            resolve(session);
        });
    });
    // Get the tokens from the session
    const tokens = {
        idToken: userSession.idToken.jwtToken,
        accessToken: userSession.accessToken.jwtToken,
        refreshToken: userSession.refreshToken.token
    };
    return tokens;
}

export const fetchWithAuth = async (url, options) => {
    const tokens = await getSessionTokens();
    if (isTokenExpired(tokens.accessToken)) {
        await refresh();
    }
    
    options.headers = {
        ...options.headers,
        'Authorization': 'Bearer ' + btoa(tokens.accessToken + ':' + tokens.refreshToken),
    };
    return fetch(url, options);
}

export const theme = createTheme({
    palette: {
        primary: { 
            main: "#24831a"
        },
        secondary: {
            main: "#d4fbcb"
        },
        error: {
            main: "#f44336"
        },
        warning: {
            main: "#ffeb3b"
        },
        info: {
            main: "#2196f3"
        },
        success: {
            main: "#4caf50"
        },
    }
});
const App = () =>{
    login();
    return(
        <div>
            <Navbar></Navbar>
            <Admin/>
        </div>
      )
}
export default App