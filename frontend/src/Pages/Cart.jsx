import React from "react"
import CartItems from '../Components/CartItems/CartItems'


const Cart = () => {  //TODO would be good to move some of the raw checkout logic from CartItems to here. CartItems should probably just be the items section not the whole checkout.
    return (
        <div>
            <CartItems />
            
        </div>
    )
}
export default Cart