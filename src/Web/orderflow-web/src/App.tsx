import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type OrderItem = {
  sku: string
  quantity: number
  unitPrice: number
}

type Order = {
  id: string
  customerName: string
  status: 'Pending' | 'Confirmed' | 'Cancelled'
  createdAtUtc: string
  total: number
  items: OrderItem[]
}

type OrderForm = {
  customerName: string
  sku: string
  quantity: string
  unitPrice: string
}

const initialForm: OrderForm = {
  customerName: '',
  sku: 'KB-001',
  quantity: '1',
  unitPrice: '99.00',
}

const apiBaseUrl = '/api'

function App() {
  const [form, setForm] = useState<OrderForm>(initialForm)
  const [orders, setOrders] = useState<Order[] | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const loadOrders = useCallback(async () => {
    try {
      const response = await fetch(`${apiBaseUrl}/orders?take=20`)
      if (!response.ok) {
        throw new Error('Unable to load recent orders.')
      }

      setOrders(await response.json() as Order[])
      setError(null)
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to load recent orders.')
      setOrders([])
    }
  }, [])

  useEffect(() => {
    // oxlint-disable-next-line react/set-state-in-effect -- Fetch completion updates UI from external I/O.
    void loadOrders()
    const timer = window.setInterval(() => void loadOrders(), 3_000)

    return () => window.clearInterval(timer)
  }, [loadOrders])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSubmitting(true)
    setError(null)

    try {
      const response = await fetch(`${apiBaseUrl}/orders`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          customerName: form.customerName,
          items: [{
            sku: form.sku,
            quantity: Number(form.quantity),
            unitPrice: Number(form.unitPrice),
          }],
        }),
      })

      if (!response.ok) {
        throw new Error(await getProblemDetail(response))
      }

      setForm(initialForm)
      await loadOrders()
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to create the order.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="dashboard">
      <header className="page-header">
        <div>
          <p className="eyebrow">Distributed order processing</p>
          <h1>OrderFlow</h1>
          <p className="intro">Create an order and follow its inventory and payment outcome.</p>
        </div>
        <button className="secondary-button" type="button" onClick={() => void loadOrders()}>
          Refresh orders
        </button>
      </header>

      <section className="panel order-form-panel" aria-labelledby="create-order-heading">
        <div className="section-heading">
          <p className="eyebrow">New order</p>
          <h2 id="create-order-heading">Submit a single-item order</h2>
        </div>

        <form onSubmit={handleSubmit}>
          <label>
            Customer name
            <input
              required
              value={form.customerName}
              onChange={(event) => setForm({ ...form, customerName: event.target.value })}
              placeholder="Demo Customer"
            />
          </label>

          <div className="item-fields">
            <label>
              SKU
              <input
                required
                value={form.sku}
                onChange={(event) => setForm({ ...form, sku: event.target.value })}
              />
            </label>
            <label>
              Quantity
              <input
                required
                min="1"
                type="number"
                value={form.quantity}
                onChange={(event) => setForm({ ...form, quantity: event.target.value })}
              />
            </label>
            <label>
              Unit price
              <input
                required
                min="0"
                step="0.01"
                type="number"
                value={form.unitPrice}
                onChange={(event) => setForm({ ...form, unitPrice: event.target.value })}
              />
            </label>
          </div>

          <button className="primary-button" disabled={isSubmitting} type="submit">
            {isSubmitting ? 'Submitting…' : 'Create order'}
          </button>
        </form>
      </section>

      <section className="panel orders-panel" aria-labelledby="recent-orders-heading">
        <div className="section-heading orders-heading">
          <div>
            <p className="eyebrow">Auto-refreshes every 3 seconds</p>
            <h2 id="recent-orders-heading">Recent orders</h2>
          </div>
          <p className="threshold-note">Under $500 succeeds; $500+ is cancelled.</p>
        </div>

        {error && <p className="error-message" role="alert">{error}</p>}

        {orders === null ? (
          <p className="empty-state">Loading orders…</p>
        ) : orders.length === 0 ? (
          <p className="empty-state">No orders yet. Create one to start the workflow.</p>
        ) : (
          <div className="orders-table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Customer</th>
                  <th>Items</th>
                  <th>Total</th>
                  <th>Status</th>
                  <th>Created</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((order) => (
                  <tr key={order.id}>
                    <td>{order.customerName}</td>
                    <td>{order.items.map((item) => `${item.sku} × ${item.quantity}`).join(', ')}</td>
                    <td>{formatCurrency(order.total)}</td>
                    <td><span className={`status status-${order.status.toLowerCase()}`}>{order.status}</span></td>
                    <td>{new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(order.createdAtUtc))}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </main>
  )
}

async function getProblemDetail(response: Response): Promise<string> {
  const body = await response.json().catch(() => null) as { detail?: string; title?: string } | null
  return body?.detail ?? body?.title ?? 'Unable to create the order.'
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value)
}

export default App
