import React from "react"
import CartItems from '../Components/CartItems/CartItems'
import Footer from "../Components/Footer/Footer"

const Cart = () => {  //TODO would be good to move some of the raw checkout logic from CartItems to here. CartItems should probably just be the items section not the whole checkout.
    return (
        <div>
            <CartItems />
            <Footer />
        </div>
    )
}
export default Cart