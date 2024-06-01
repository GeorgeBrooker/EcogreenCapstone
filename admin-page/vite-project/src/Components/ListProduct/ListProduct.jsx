import React, { useEffect, useState } from "react";
import './ListProduct.css';
import Modal from '../Modal/Modal';
import AddProduct from "../AddProduct/AddProduct";
import ModifyProduct from "../ModifyProduct/ModifyProduct";
import { serverUri, apiEndpoint, theme, fetchWithAuth } from "../../App.jsx";
import {Button, ThemeProvider, CircularProgress, Switch, FormControlLabel} from "@mui/material";
import {Api} from "@mui/icons-material";

const ListProduct = () => {
    const [showArchivedProducts, setShowArchivedProducts] = useState(false); // Show archived products or not
    const [archivedProducts, setArchivedProducts] = useState([]); // Archived products
    const [activeProducts, setActiveProducts] = useState([]); // Active products
    const [selectedProducts, setSelectedProducts] = useState(new Set()); // Selected products
    const [showAddProductModal, setShowAddProductModal] = useState(false); // Show add product modal or not
    const [showModifyProductModal, setShowModifyProductModal] = useState(false); // Show modify product modal or not
    const [selectedProductId, setSelectedProductId] = useState(null); // Selected product id
    // loading spinner states
    const [archiveExecuting, setArchiveExecuting] = useState(false);
    const [productUpdating, setProductUpdating] = useState(false);
    
    const testSpin = () => {
        setTestSpining(true);
        setTimeout(() => {
            setTestSpining(false);
        }, 2000);
    };
    
    const fetchProductInfo = async () => {
        setProductUpdating(true)
        const response = await fetchWithAuth(`${serverUri}${apiEndpoint}/GetAllStock`, );
        const data = await response.json();
        localStorage.setItem("allProducts", JSON.stringify(data));// Cache product data locally, refetch on modification
        
        // Separate active and archived products for display purposes
        const activeProducts = data.filter(product => product.active);
        const archivedProducts = data.filter(product => !product.active);
        
        setActiveProducts(activeProducts);
        setArchivedProducts(archivedProducts);
        setProductUpdating(false);
    };

    useEffect(() => {
        fetchProductInfo();
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
    
    const archiveHandler = async () => {
        setArchiveExecuting(true); // show spinner
        
        const failedUpdates = [];
        for (let id of selectedProducts) {
            try {
                const response = await fetchWithAuth(`${serverUri}${apiEndpoint}/SetStockArchiveState/${id}`, {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ active: showArchivedProducts }),
                });
                if (!response.ok) {
                    failedUpdates.push(id + " : " + await response.text());
                    throw new Error(id);
                }
                else {
                    fetchProductInfo();
                    alert("Archive state changed successfully");
                }
            } catch (error) {
                console.error('Error changing archive state of product:', error);
            }
        }
        
        if (failedUpdates.length === 0) {
            setActiveProducts(activeProducts.filter(product => !selectedProducts.has(product.id)));
            setSelectedProducts(new Set());
        } else {
            alert(`Failed to change archive state of products with Errors:\n\n${failedUpdates.join('\n\n')}`);
        }
        setArchiveExecuting(false); // hide spinner
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
        const response = await fetchWithAuth(`${serverUri}${apiEndpoint}/UpdateStock/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify( transformedData),
        });
        if (response.ok) {
            const updatedProduct = await response.json();
            setActiveProducts(prevProducts => prevProducts.map(product => {
                return product.id === id ? { ...product, ...updatedProduct } : product;
            }));
            alert("Modification successful");
            console.log("Updated product data:", transformedData);
             
        } else {
            alert("Modification failed");
        }
        fetchProductInfo();
        setShowModifyProductModal(false);
    };

    return (
        <ThemeProvider theme={theme}>
            
            <div className={"page-headers"}>
                <h1 className="title">{showArchivedProducts ? 'Archived Products' : 'Active Products'}</h1>
                <div className={"button-container"}>
                    <FormControlLabel
                        style={{marginRight: 'auto'}}
                        className={"show-archived-products-switch"}
                        control={
                            <Switch
                                checked={showArchivedProducts}
                                onChange={() => setShowArchivedProducts(!showArchivedProducts)}
                                name="showArchivedProducts"
                                color="primary"
                            />
                        }
                        label={showArchivedProducts ? 'Hide Archived Products' : 'Show Archived Products'}
                    />
                    <Button style={{marginRight: '10px'}} onClick={() => setShowAddProductModal(true)} variant={"contained"}
                            className="add-product-button">Add Product</Button>

                    <Button className="delete-selected-button" onClick={() => archiveHandler()} variant="contained"
                            disabled={archiveExecuting} color={"error"}>
                        {archiveExecuting ?
                            <CircularProgress size={20} color="inherit"/>
                            :
                            (showArchivedProducts ? 'Unarchive' : 'Archive')
                        }
                    </Button>
                </div>
            </div>
            <div className="list-product">
                
                <Modal isOpen={showAddProductModal} onClose={() => setShowAddProductModal(false)}>
                    <AddProduct onClose={() => setShowAddProductModal(false)}/>
                </Modal>
                
                <Modal isOpen={showModifyProductModal} onClose={() => setShowModifyProductModal(false)}>
                    <ModifyProduct productId={selectedProductId} onClose={() => setShowModifyProductModal(false)}
                                   onSave={handleSaveChanges}/>
                </Modal>
                
                <div className="listproduct-format listproduct-header">
                    <p className={"col-1"}>Product</p>
                    <p className={"col-2"}>Title</p>
                    <p className={"col-3"}>Old Price</p>
                    <p className={"col-4"}>New Price</p>
                    <p className={"col-5"}>Stock</p>
                    <p className={"col-6"}>{showArchivedProducts ? "Unarchive" : "Archive"}</p>
                </div>
                
                {productUpdating && <div style={{ 
                    display: 'flex',
                    flexDirection: 'column',
                    justifyContent: 'center',
                    alignItems: 'center',
                    height: '100vh'
                }}>
                    <CircularProgress/>
                    <span>Loading...</span>
                </div>}

                {!showArchivedProducts && !productUpdating ? (
                    <div className="listproduct-allproducts">
                        {activeProducts.map((product, index) => {
                            const uriName = product.name.replace(/\s/g, '-');
                            const productUri = `${product.photoUri}${uriName}-${product.id}/1.jpeg`;
                            const discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage) / 100));
    
                            const handleProductClick = (event, productId) => {
    
                                if (event.target.type !== 'checkbox') {
                                    handleEditProduct(productId);
                                }
                            };
                            
                            return (
                                <div key={index} className="listproduct-format listproduct-item"
                                     onClick={(e) => handleProductClick(e, product.id)}>
                                    <img src={productUri} alt="" className="listproduct-product-icon col-1"/>
                                    <p className={"col-2"}>{product.name}</p>
                                    <p className={"col-3"}>${product.price}</p>
                                    <p className={"col-4"}>${discountedPrice}</p>
                                    <p className={"col-5"}>{product.totalStock}</p>
    
                                    <input
                                        type="checkbox"
                                        className="listproduct-checkbox col-6"
                                        checked={selectedProducts.has(product.id)}
                                        onChange={(e) => {
                                            e.stopPropagation();
                                            toggleProductSelection(product.id);
                                        }}
                                    />
                                </div>
                            );
                        })}
                        {(activeProducts.length === 0 && !productUpdating) && <p className="no-products-message">No products available</p>}
                    </div>
                    ) : null}
                {!productUpdating && showArchivedProducts ? (
                    <div className="listproduct-allproducts">
                        {archivedProducts.map((product, index) => {
                            const uriName = product.name.replace(/\s/g, '-');
                            const productUri = `${product.photoUri}${uriName}-${product.id}/1.jpeg`;
                            const discountedPrice = Number(product.price) * (1 - (Number(product.discountPercentage) / 100));
    
                            const handleProductClick = (event, productId) => {
    
                                if (event.target.type !== 'checkbox') {
                                    handleEditProduct(productId);
                                }
                            };
                            return (
                                <div key={index} className="listproduct-format"
                                     onClick={(e) => handleProductClick(e, product.id)}>
                                    <img src={productUri} alt="" className="listproduct-product-icon col-1"/>
                                    <p className={"col-2"}>{product.name}</p>
                                    <p className={"col-3"}>${product.price}</p>
                                    <p className={"col-4"}>${discountedPrice}</p>
                                    <p className={"col-5"}>{product.totalStock}</p>
    
                                    <input
                                        type="checkbox"
                                        className="listproduct-checkbox col-6"
                                        checked={selectedProducts.has(product.id)}
                                        onChange={(e) => {
                                            e.stopPropagation();
                                            toggleProductSelection(product.id);
                                        }}
                                    />
                                </div>
                            );
                        })}
                        {(archivedProducts.length === 0 && !productUpdating) && <p className="no-products-message">No archived products</p>}
                    </div>
                ) : null}
            </div>
        </ThemeProvider>
    );
};

export default ListProduct;
