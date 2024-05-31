import React from "react";
import Navbar from "./Components/Navbar/Navbar";
import Admin from './Pages/Admin/Admin'
import { createTheme } from "@mui/material/styles";
export const serverUri = process.env.NODE_ENV === 'production'
    ? "https://nn8hvsrhhk.execute-api.ap-southeast-2.amazonaws.com"
    : "http://localhost:3000";

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
  return(
    <div>
      <Navbar></Navbar>
      <Admin/>
    </div>
  )
}
export default App