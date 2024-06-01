import { CognitoUserPool, CognitoUser, CognitoUserSession, CognitoIdToken, CognitoAccessToken, CognitoRefreshToken } from 'amazon-cognito-identity-js';
import {jwtDecode} from "jwt-decode";
import 'globalthis/auto';

if (typeof window !== 'undefined') {
    window.global = window;
}
const redirectUri = window.location.origin;
const userPoolId = 'ap-southeast-2_IIwqJRcot';
const clientId = '3uen21ajpr2814p72q2ghuujaj';
const cognitoDomain = 'https://kashishadmin.auth.ap-southeast-2.amazoncognito.com';
const loginQuery = `/login?response_type=code&client_id=${clientId}&redirect_uri=${window.location.origin}`;
const loginUri = cognitoDomain + loginQuery;

const poolData = {
    UserPoolId: userPoolId, // Your user pool id here
    ClientId: clientId // Your client id here
};
const userPool = new CognitoUserPool(poolData);
let cognitoUser = userPool.getCurrentUser();
const refresh = async () => {
    cognitoUser.getSession((err, session) => {
        if (err) {
            console.error(err);
            return;
        }
        console.log('session validity: ' + session.isValid());
        if (!session.isValid()) {
            cognitoUser.refreshSession(session.getRefreshToken(), (err, session) => {
                if (err) {
                    console.error(err);
                    return;
                }
                console.log('session validity: ' + session.isValid());
            });
        }
    });

}

const isTokenExpired = (token) => {
    const decodedToken = jwtDecode(token);
    return decodedToken.exp < Date.now() / 1000;
}
const login = async () => {
    // Check for auth code in URL
    const urlParams = new URLSearchParams(window.location.search);
    const code = urlParams.get('code');
    if (code) {
        await exchangeCodeForTokens(code);
    }
    cognitoUser = userPool.getCurrentUser();
    let userSession = null;

    // if no user is stored, check if there are stored tokens and load the user from them
    if (cognitoUser == null && localStorage.getItem('id_token') && localStorage.getItem('access_token') && localStorage.getItem('refresh_token')) {
        const idToken = new CognitoIdToken({IdToken: localStorage.getItem('id_token')});
        const accessToken = new CognitoAccessToken({AccessToken: localStorage.getItem('access_token')});
        const refreshToken = new CognitoRefreshToken({RefreshToken: localStorage.getItem('refresh_token')});

        console.log(idToken, accessToken, refreshToken)

        const sessionData = {
            IdToken: idToken,
            AccessToken: accessToken,
            RefreshToken: refreshToken
        };
        const session = new CognitoUserSession(sessionData);

        console.log(jwtDecode(localStorage.getItem('id_token')));
        const userName = jwtDecode(idToken.jwtToken)['cognito:username'];

        const userData = {
            Username: userName,
            Pool: userPool
        };
        const refreshedUser = new CognitoUser(userData);
        refreshedUser.setSignInUserSession(session);
    }
    // Process user session
    cognitoUser = userPool.getCurrentUser()
    if (cognitoUser) {
        cognitoUser.getSession((err, session) => {
            if (err) {
                console.error(err);
                return;
            }
            console.log('session validity: ' + session.isValid());
            userSession = session;
        });
    }

    // Redirect to login if not logged in
    if (!cognitoUser || !userSession) {
        alert("You are not logged in, please log in to continue")
        window.location.href = loginUri;
    } else {
        const urlParams = new URLSearchParams(window.location.search);
        const code = urlParams.get('code');
        if (code) {
            exchangeCodeForTokens(code);
        }
    }

    async function exchangeCodeForTokens(code) {
        const requestBody = {
            grant_type: 'authorization_code',
            client_id: clientId,
            code: code,
            redirect_uri: window.location.origin
        };

        const response = await fetch(cognitoDomain + '/oauth2/token', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: new URLSearchParams(requestBody)
        });
        if (!response.ok) {
            throw new Error('Failed to exchange code for tokens');
        }
        const data = await response.json();

        const idToken = new CognitoIdToken({IdToken: data.id_token});
        const accessToken = new CognitoAccessToken({AccessToken: data.access_token});
        const refreshToken = new CognitoRefreshToken({RefreshToken: data.refresh_token});

        const sessionData = {
            IdToken: idToken,
            AccessToken: accessToken,
            RefreshToken: refreshToken
        };
        const session = new CognitoUserSession(sessionData);

        const userName = jwtDecode(data.id_token)['cognito:username'];
        const userData = {
            Username: userName,
            Pool: userPool
        };

        const loggedUser = new CognitoUser(userData);
        loggedUser.setSignInUserSession(session);
    }
}

export { cognitoUser, login, userPool, refresh, isTokenExpired };