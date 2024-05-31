import React, { useEffect, useState } from "react";
import './ListProduct.css';
import Modal from '../Modal/Modal';
import AddProduct from "../AddProduct/AddProduct";
import ModifyProduct from "../ModifyProduct/ModifyProduct";
import { serverUri } from "../../App.jsx";
import ConfirmationModal from "../Confirmmodal/Confirmmodal";

const ListProduct = () => {
    const [allProducts, setAllProducts] = useState([]);
    const [selectedProducts, setSelectedProducts] = useState(new Set());
    const [showAddProductModal, setShowAddProductModal] = useState(false);
    const [showModifyProductModal, setShowModifyProductModal] = useState(false);
    const [selectedProductId, setSelectedProductId] = useState(null);
    const [showConfirmModal, setShowConfirmModal] = useState(false); 
    

    useEffect(() => {
        const fetchInfo = async () => {
            const response = await fetch(`${serverUri}/api/shop/GetAllStock`);
            const data = await response.json();
            setAllProducts(data);
        };
        fetchInfo();
    }, []);

    const toggleProductSelection = (id) => {
        const newSelection = new Set(selectedProducts);
        if (newSelection.has(id)) {
            newSelection.delete(id);
        } else {
            newSelection.add(id);
        }
        setSelectedProducts(newSelection);
    };

    const deleteSelectedProducts = async () => {
        const failedDeletes = [];
        for (let id of selectedProducts) {
            try {
                const response = await fetch(`${serverUri}/api/shop/DeleteStock/${id}`, {
                    method: 'DELETE'
                });
                if (!response.ok) {
                    throw new Error(`Failed to delete product with ID ${id}`);
                }
            } catch (error) {
                console.error('Error deleting product:', error);
                failedDeletes.push(id);
            }
        }
        if (failedDeletes.length === 0) {
            setAllProducts(allProducts.filter(product => !selectedProducts.has(product.id)));
            
            setSelectedProducts(new Set());
            setShowConfirmModal(false);
            
        } else {
            alert(`Failed to delete products with IDs: ${failedDeletes.join(', ')}`);
        }
    };

    const handleEditProduct = (productId) => {
        setSelectedProductId(productId);
        setShowModifyProductModal(true);
    };
    function transformProductDetails(details) {
        return {
            name: details.name,  
            description: details.description,
            price: details.price,
            discountPercentage: details.discount,  
            totalStock: details.quantity
        };
    }
    

    const handleSaveChanges = async (id, updatedData) => {
        const transformedData = transformProductDetails(updatedData); 
        const response = await fetch(`${serverUri}/api/shop/UpdateStock/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify( transformedData),
        });
        if (response.ok) {
            const updatedProduct = await response.json();
            setAllProducts(prevProducts => prevProducts.map(product => {
                return product.id === id ? { ...product, ...updatedProduct } : product;
            }));
            alert("Modification successful");
            window.location.reload()
            console.log("Updated product data:", transformedData);
             
        } else {
            alert("Modification failed");
        }
        setShowModifyProductModal(false);
    };

    return (
        <div className="list-product">
            <h1 className="title">All Products List</h1>
            <button onClick={() => setShowAddProductModal(true)} className="add-product-button">Add Product</button>
            <button onClick={() => setShowConfirmModal(true)} className="delete-selected-button">Delete Selected</button>

            
            <Modal isOpen={showAddProductModal} onClose={() => setShowAddProductModal(false)}>
                <AddProduct />
            </Modal>
            

            <Modal isOpen={showModifyProductModal} onClose={() => setShowModifyProductModal(false)}>
                <ModifyProduct productId={selectedProductId} onClose={() => setShowModifyProductModal(false)} onSave={handleSaveChanges} />
            </Modal>

            <ConfirmationModal
                isOpen={showConfirmModal}
                onClose={() => setShowConfirmModal(false)}
                onConfirm={deleteSelectedProducts}
            />
            
            <div className="listproduct-format-main">
                <p>Product</p>
                <p>Title</p>
                <p>Old Price</p>
                <p>New Price</p>
                <p>Stock</p>
                
                <p>Remove</p>
            </div>
            
            <div className="listproduct-allproducts">
                {allProducts.map((product, index) => {
                    const uriName = product.name.replace(/\s/g, '-');
                    const productUri = `${product.photoUri}${uriName}-${product.id}1.jpeg`;
                    const discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage) / 100));

                    const handleProductClick = (event, productId) => {
                         
                        if (event.target.type !== 'checkbox') {
                            handleEditProduct(productId);
                        }
                    };
                    
            

                    return (
                        <div key={index} className="listproduct-format" onClick={(e) => handleProductClick(e, product.id)}>
                            <img src={productUri} alt="" className="listproduct-product-icon" />
                            <p>{product.name}</p>
                            <p>${product.price}</p>
                            <p>${discountedPrice}</p>
                            <p>{product.totalStock}</p>
                       
                            <input 
                                type="checkbox" 
                                className="listproduct-checkbox"
                                checked={selectedProducts.has(product.id)}
                                onChange={(e) => {
                                    e.stopPropagation();
                                    toggleProductSelection(product.id);
                                }}
                            />
                        </div>
                    );
                })}
            </div>
            
        </div>
    );
};

export default ListProduct;
