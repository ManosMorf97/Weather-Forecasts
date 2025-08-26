const Sequelize = require('sequelize');
module.exports = function(sequelize, DataTypes) {
  return sequelize.define('Ratings', {
    Email: {
      type: DataTypes.STRING(450),
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Users',
        key: 'Email'
      }
    },
    City_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    Site_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    Timeslot_Id: {
      type: DataTypes.INTEGER,
      allowNull: false,
      primaryKey: true,
      references: {
        model: 'Predictions',
        key: 'Timeslot_Id'
      }
    },
    Rating_Value: {
      type: DataTypes.INTEGER,
      allowNull: false
    }
  }, {
    sequelize,
    tableName: 'Ratings',
    schema: 'dbo',
    timestamps: false,
    indexes: [
      {
        name: "IX_Ratings_City_Id_Site_Id_Timeslot_Id",
        fields: [
          { name: "City_Id" },
          { name: "Site_Id" },
          { name: "Timeslot_Id" },
        ]
      },
      {
        name: "PK_Ratings",
        unique: true,
        fields: [
          { name: "Email" },
          { name: "City_Id" },
          { name: "Site_Id" },
          { name: "Timeslot_Id" },
        ]
      },
    ]
  });
};
