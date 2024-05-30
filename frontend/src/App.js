
import './App.css';
import Navbar from './Components/Navbar/Navbar';
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Shop from './Pages/Shop';
import Product from './Pages/Product';
import Cart from './Pages/Cart';
import LoginSignup from './Pages/LoginSignup';
import Home from './Pages/Home';
import ContactUs from './Pages/ContactUs';
 
import Detail from './Pages/Detail'
import OrderSuccess from './Pages/OrderSuccess';
import OrderFail from './Pages/OrderFail';


function App() {
  return (
    <div>
      <BrowserRouter>
      <Navbar/>
      <Routes>
        <Route path='/' element={<Home />}/>
        <Route path='/home' element={<Home />}/>
        <Route path='/shop' element={<Shop />}/>
        <Route path='/contactUs' element={<ContactUs />}/> 
        <Route path='product' element={<Product />}>
          <Route path=':productId' element={<Product/>}/>
        </Route>
        <Route path='/cart' element={<Cart />}/>
        <Route path='/login' element={<LoginSignup />}/>
        <Route  path='/detail' element={<Detail />}/>
        <Route path='/ordersuccess' element={<OrderSuccess />}/>
        <Route path='/orderfail' element={<OrderFail />}/>
      </Routes>
  
      </BrowserRouter>
    </div>
  );
}

export default App;
