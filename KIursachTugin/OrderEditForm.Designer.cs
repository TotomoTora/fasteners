namespace KIursachTugin
{
    partial class OrderEditForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OrderEditForm));
            this.dgvCart = new System.Windows.Forms.DataGridView();
            this.lblTotal = new System.Windows.Forms.Label();
            this.btnSaveOrder = new System.Windows.Forms.Button();
            this.btnAddToCart = new System.Windows.Forms.Button();
            this.btnRemoveFormCart = new System.Windows.Forms.Button();
            this.txtSeller = new System.Windows.Forms.TextBox();
            this.dgvProductsList = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductsList)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCart
            // 
            this.dgvCart.BackgroundColor = System.Drawing.Color.White;
            this.dgvCart.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvCart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCart.Location = new System.Drawing.Point(510, 37);
            this.dgvCart.Name = "dgvCart";
            this.dgvCart.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCart.Size = new System.Drawing.Size(569, 305);
            this.dgvCart.TabIndex = 8;
            this.dgvCart.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCart_CellContentClick);
            this.dgvCart.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCart_CellEndEdit);
            this.dgvCart.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvCart_EditingControlShowing);
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTotal.Location = new System.Drawing.Point(400, 356);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(45, 15);
            this.lblTotal.TabIndex = 9;
            this.lblTotal.Text = "Сумма";
            // 
            // btnSaveOrder
            // 
            this.btnSaveOrder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(144)))), ((int)(((byte)(226)))));
            this.btnSaveOrder.FlatAppearance.BorderSize = 0;
            this.btnSaveOrder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveOrder.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnSaveOrder.Location = new System.Drawing.Point(201, 348);
            this.btnSaveOrder.Name = "btnSaveOrder";
            this.btnSaveOrder.Size = new System.Drawing.Size(183, 28);
            this.btnSaveOrder.TabIndex = 11;
            this.btnSaveOrder.Text = "Сохранить заказ";
            this.btnSaveOrder.UseVisualStyleBackColor = false;
            this.btnSaveOrder.Click += new System.EventHandler(this.btnSaveOrder_Click);
            // 
            // btnAddToCart
            // 
            this.btnAddToCart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnAddToCart.FlatAppearance.BorderSize = 0;
            this.btnAddToCart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddToCart.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnAddToCart.Location = new System.Drawing.Point(12, 348);
            this.btnAddToCart.Name = "btnAddToCart";
            this.btnAddToCart.Size = new System.Drawing.Size(183, 28);
            this.btnAddToCart.TabIndex = 12;
            this.btnAddToCart.Text = "Добавить";
            this.btnAddToCart.UseVisualStyleBackColor = false;
            this.btnAddToCart.Click += new System.EventHandler(this.btnAddToCart_Click);
            // 
            // btnRemoveFormCart
            // 
            this.btnRemoveFormCart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnRemoveFormCart.FlatAppearance.BorderSize = 0;
            this.btnRemoveFormCart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveFormCart.Font = new System.Drawing.Font("Segoe UI", 9.75F);
            this.btnRemoveFormCart.Location = new System.Drawing.Point(898, 348);
            this.btnRemoveFormCart.Name = "btnRemoveFormCart";
            this.btnRemoveFormCart.Size = new System.Drawing.Size(183, 28);
            this.btnRemoveFormCart.TabIndex = 13;
            this.btnRemoveFormCart.Text = "Удалить";
            this.btnRemoveFormCart.UseVisualStyleBackColor = false;
            this.btnRemoveFormCart.Click += new System.EventHandler(this.btnRemoveFromCart_Click);
            // 
            // txtSeller
            // 
            this.txtSeller.BackColor = System.Drawing.Color.White;
            this.txtSeller.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSeller.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.txtSeller.Location = new System.Drawing.Point(12, 12);
            this.txtSeller.Name = "txtSeller";
            this.txtSeller.ReadOnly = true;
            this.txtSeller.Size = new System.Drawing.Size(157, 15);
            this.txtSeller.TabIndex = 15;
            // 
            // dgvProductsList
            // 
            this.dgvProductsList.BackgroundColor = System.Drawing.Color.White;
            this.dgvProductsList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dgvProductsList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProductsList.Location = new System.Drawing.Point(12, 37);
            this.dgvProductsList.Name = "dgvProductsList";
            this.dgvProductsList.ReadOnly = true;
            this.dgvProductsList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvProductsList.Size = new System.Drawing.Size(492, 305);
            this.dgvProductsList.TabIndex = 16;
            this.dgvProductsList.CellContentDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProductsList_CellDoubleClick);
            // 
            // OrderEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(247)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(1091, 385);
            this.Controls.Add(this.dgvProductsList);
            this.Controls.Add(this.txtSeller);
            this.Controls.Add(this.btnRemoveFormCart);
            this.Controls.Add(this.btnAddToCart);
            this.Controls.Add(this.btnSaveOrder);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.dgvCart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OrderEditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Создание";
            this.Load += new System.EventHandler(this.OrderEditForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductsList)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvCart;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnSaveOrder;
        private System.Windows.Forms.Button btnAddToCart;
        private System.Windows.Forms.Button btnRemoveFormCart;
        private System.Windows.Forms.TextBox txtSeller;
        private System.Windows.Forms.DataGridView dgvProductsList;
    }
}