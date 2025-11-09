// Array para almacenar los productos agregados
let productosAgregados = [];

// Constante de IVA (13%)
const IVA_PORCENTAJE = 0.13;

// Elementos del DOM
const btnBuscarProducto = document.getElementById('btnBuscarProducto');
const inputCodigoProducto = document.getElementById('codigoProducto');
const tbodyProductos = document.getElementById('tbodyProductos');
const mensajeBusqueda = document.getElementById('mensajeBusqueda');
const mensajeTablaVacia = document.getElementById('mensajeTablaVacia');
const btnRegistrarVenta = document.getElementById('btnRegistrarVenta');
const mensajeError = document.getElementById('mensajeError');

// Event Listeners
document.addEventListener('DOMContentLoaded', function() {
    btnBuscarProducto.addEventListener('click', buscarProducto);
    
    inputCodigoProducto.addEventListener('keypress', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            buscarProducto();
        }
    });

    btnRegistrarVenta.addEventListener('click', registrarVenta);
});

async function buscarProducto() {
    const codigo = inputCodigoProducto.value.trim();
    
    if (!codigo) {
        mostrarMensajeBusqueda('Por favor ingrese un código de producto', 'warning');
        return;
    }

    // Verificar si el producto ya fue agregado
    if (productosAgregados.find(p => p.idPro === parseInt(codigo))) {
        mostrarMensajeBusqueda('Este producto ya fue agregado a la venta', 'warning');
        inputCodigoProducto.value = '';
        inputCodigoProducto.focus();
        return;
    }

    try {
        // Deshabilitar botón mientras busca
        btnBuscarProducto.disabled = true;
        btnBuscarProducto.textContent = 'Buscando...';

        const response = await fetch(`/Ventas/GetProducto?id=${codigo}`);
        const result = await response.json();

        if (result.success && result.data) {
            agregarProducto(result.data);
            mostrarMensajeBusqueda('Producto agregado correctamente', 'success');
            inputCodigoProducto.value = '';
            inputCodigoProducto.focus();
        } else {
            mostrarMensajeBusqueda(result.message || 'Producto no encontrado', 'danger');
        }
    } catch (error) {
        mostrarMensajeBusqueda('Error al buscar el producto: ' + error.message, 'danger');
    } finally {
        btnBuscarProducto.disabled = false;
        btnBuscarProducto.textContent = 'Buscar';
    }
}

function agregarProducto(producto) {
    productosAgregados.push({
        idPro: producto.idPro,
        producto: producto.producto,
        precio: producto.precio,
        cantidad: 1
    });

    renderizarTabla();
}

function quitarProducto(idPro) {
    if (confirm('¿Está seguro de quitar este producto de la venta?')) {
        productosAgregados = productosAgregados.filter(p => p.idPro !== idPro);
        renderizarTabla();
    }
}

function actualizarCantidad(idPro, cantidad) {
    const producto = productosAgregados.find(p => p.idPro === idPro);
    if (producto) {
        producto.cantidad = parseInt(cantidad) || 1;
        renderizarTabla();
    }
}

function renderizarTabla() {
    if (productosAgregados.length === 0) {
        tbodyProductos.innerHTML = '';
        mensajeTablaVacia.style.display = 'block';
        calcularTotales();
        return;
    }

    mensajeTablaVacia.style.display = 'none';

    let html = '';
    productosAgregados.forEach(producto => {
        const subtotal = producto.precio * producto.cantidad;
        const iva = subtotal * IVA_PORCENTAJE;
        const total = subtotal + iva;

        html += `
            <tr>
                <td><strong>#${producto.idPro}</strong></td>
                <td>${producto.producto}</td>
                <td class="text-end">$${producto.precio.toFixed(2)}</td>
                <td class="text-center">
                    <input type="number" 
                           class="form-control form-control-sm text-center" 
                           style="width: 80px; margin: 0 auto;"
                           value="${producto.cantidad}" 
                           min="1" 
                           onchange="actualizarCantidad(${producto.idPro}, this.value)">
                </td>
                <td class="text-end">$${iva.toFixed(2)}</td>
                <td class="text-end"><strong>$${total.toFixed(2)}</strong></td>
                <td class="text-center">
                    <button type="button" 
                            class="btn btn-sm btn-outline-danger" 
                            onclick="quitarProducto(${producto.idPro})"
                            title="Quitar">
                        X
                    </button>
                </td>
            </tr>
        `;
    });

    tbodyProductos.innerHTML = html;
    calcularTotales();
}

function calcularTotales() {
    let subtotalGeneral = 0;
    let ivaGeneral = 0;
    let totalGeneral = 0;

    productosAgregados.forEach(producto => {
        const subtotal = producto.precio * producto.cantidad;
        const iva = subtotal * IVA_PORCENTAJE;
        const total = subtotal + iva;

        subtotalGeneral += subtotal;
        ivaGeneral += iva;
        totalGeneral += total;
    });

    document.getElementById('resumenSubtotal').textContent = '$' + subtotalGeneral.toFixed(2);
    document.getElementById('resumenIva').textContent = '$' + ivaGeneral.toFixed(2);
    document.getElementById('resumenTotal').textContent = '$' + totalGeneral.toFixed(2);
}

/**
 * Registrar venta
 */
function registrarVenta() {
    if (productosAgregados.length === 0) {
        mostrarMensajeError('Debe agregar al menos un producto para registrar la venta');
        return;
    }

    // Construir el modelo para enviar al servidor
    const detalles = productosAgregados.map(p => ({
        idPro: p.idPro,
        cantidad: p.cantidad
    }));

    // Crear un formulario dinámico para enviar los datos
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Ventas/Create';

    // Agregar token anti-forgery
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tokenInput) {
        const token = document.createElement('input');
        token.type = 'hidden';
        token.name = '__RequestVerificationToken';
        token.value = tokenInput.value;
        form.appendChild(token);
    }

    detalles.forEach((detalle, index) => {
        const idProInput = document.createElement('input');
        idProInput.type = 'hidden';
        idProInput.name = `Detalles[${index}].IdPro`;
        idProInput.value = detalle.idPro;
        form.appendChild(idProInput);

        const cantidadInput = document.createElement('input');
        cantidadInput.type = 'hidden';
        cantidadInput.name = `Detalles[${index}].Cantidad`;
        cantidadInput.value = detalle.cantidad;
        form.appendChild(cantidadInput);
    });

    document.body.appendChild(form);
    form.submit();
}

/**
 * Mostrar mensaje de búsqueda
 */
function mostrarMensajeBusqueda(mensaje, tipo) {
    const clases = {
        'success': 'alert-success',
        'danger': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    };

    mensajeBusqueda.innerHTML = `
        <div class="alert ${clases[tipo]} alert-dismissible fade show" role="alert">
            ${mensaje}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;

    // Auto-ocultar después de 3 segundos
    setTimeout(() => {
        mensajeBusqueda.innerHTML = '';
    }, 3000);
}

function mostrarMensajeError(mensaje) {
    mensajeError.textContent = mensaje;
    mensajeError.style.display = 'block';

    setTimeout(() => {
        mensajeError.style.display = 'none';
    }, 5000);
}
